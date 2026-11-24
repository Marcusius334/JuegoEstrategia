using System.Collections.Generic;
using UnityEngine;

public class AIPlayer : Player
{
    [Header("Configuración")]
    public Troop troopPrefab;
    [SerializeField] private HexGridManager gridManager;
    [SerializeField] private TacticalInfluenceManager influenceManager;

    [Header("Tropas Activas")]
    public List<Troop> myTroops = new List<Troop>();

    // Arquitectura de Árboles de Comportamiento
    private Blackboard _blackboard;
    private BTNode _constructionTree;
    private BTNode _movementTree;
    private BTNode _attackTree;

    private void Start()
    {
        /* if (gameManager != null)
        {
            chatarra = gameManager.ia_chatarra;
            creditos = gameManager.ia_creditos;
            tritio = gameManager.ia_tritio;
            cristal = gameManager.ia_cristal;
        } */

        if (gridManager == null) gridManager = FindFirstObjectByType<HexGridManager>();
        if (influenceManager == null) influenceManager = FindFirstObjectByType<TacticalInfluenceManager>();

        _blackboard = new Blackboard();
        BuildBehaviorTrees();
    }

    private void BuildBehaviorTrees()
    {
        // =========================================
        // 1. ÁRBOL DE CONSTRUCCIÓN
        // =========================================
        _constructionTree = new Selector(new List<BTNode>
        {
            new Sequence(new List<BTNode>
            {
                new ConditionNode(CanAffordTroop),
                new ConditionNode(FindSpawnCell),
                new ActionNode(BuildTroopAction)
            }),
            new ActionNode(() =>
            {
                Debug.Log("IA (Construcción): Sin recursos o sin casillas válidas.");
                return NodeState.Success;
            })
        });

        // =========================================
        // 2. ÁRBOL DE MOVIMIENTO (Por tropa)
        // =========================================
        _movementTree = new Selector(new List<BTNode>
        {
            // Rama 1 Importante: Retirada si tiene menos del 30% de vida
            new Sequence(new List<BTNode>
            {
                new ConditionNode(IsCurrentTroopLowHealth),
                new AlwaysSucceed(new ActionNode(RefreshTacticalMapsAction)),
                new ConditionNode(FindRetreatCell),
                new ActionNode(MoveCurrentTroopAction)
            }),
            // Rama 2 Prioridad media: Avanzar hacia el enemigo mas cercano si no esta en rango
            new Sequence(new List<BTNode>
            {
                new ConditionNode(NoEnemyInAttackRange),
                new ConditionNode(FindAdvanceCellTowardEnemy),
                new ActionNode(MoveCurrentTroopAction)
            }),
            // Rama 3 (Fallback): Mantener posición
            new ActionNode(() => NodeState.Success)
        });

        // =========================================
        // 3. ÁRBOL DE ATAQUE (Por tropa)
        // =========================================
        _attackTree = new Selector(new List<BTNode>
        {
            // Rama 1 (Prioridad alta): Rematar un objetivo en rango
            new Sequence(new List<BTNode>
            {
                new ConditionNode(FindLethalTargetInRange),
                new ActionNode(AttackTargetAction)
            }),
            // Rama 2 (Prioridad media): Atacar al enemigo en rango al que mas daño haga
            new Sequence(new List<BTNode>
            {
                new ConditionNode(FindBestTargetInRange),
                new ActionNode(AttackTargetAction)
            }),
            // Rama 3 (Fallback): No hay nadie en rango
            new ActionNode(() => NodeState.Success)
        });
    }

    // =========================================
    // MÉTODOS LLAMADOS POR TURNSMANAGER
    // =========================================

    public void PerformConstructionPhase()
    {
        Debug.Log("IA: Evaluando BT de Construcción...");
        if (_blackboard == null)
        {
            _blackboard = new Blackboard();
            BuildBehaviorTrees();
        }
        RefreshTroopLists();
        if (_constructionTree == null)
        {
            Debug.LogError("El árbol de construcción no se ha inicializado.");
            return;
        }
        _constructionTree.Evaluate();
    }

    // =========================================
    // FASE 2: MOVIMIENTO
    // =========================================
    private HexCell ChoosBestCell(Troop troop)
    {
        return gridManager.GetCell(PathfindingDijkstra.GetBestPosition(gridManager, troop, influenceManager));  //cambiar Pathfinding por A* y en el parentesis falta la variable que será la cordenada de donde esta la tropa
    }
    public void PerformMovementPhase()
    {
        if (_blackboard == null)
        {
            _blackboard = new Blackboard();
            BuildBehaviorTrees();
        }
        Debug.Log("IA: Evaluando BT de Movimiento...");
        RefreshTroopLists();

        foreach (Troop troop in myTroops)
        {
            if (troop == null || troop.currentCell == null) continue;
            _blackboard.Set("CurrentTroop", troop);
            _movementTree.Evaluate();
        }
    }

    public void PerformAttackPhase()
    {
        if (_blackboard == null)
        {
            _blackboard = new Blackboard();
            BuildBehaviorTrees();
        }
        Debug.Log("IA: Evaluando BT de Ataque...");
        RefreshTroopLists();

        foreach (Troop troop in myTroops)
        {
            if (troop == null || troop.currentCell == null) continue;
            _blackboard.Set("CurrentTroop", troop);
            _attackTree.Evaluate();
        }
    }

    // =========================================
    // UTILIDADES DE TABLERO Y TROPAS
    // =========================================

    private void RefreshTroopLists()
    {
        myTroops.Clear();
        List<Troop> enemyTroops = new List<Troop>();

        Troop[] allTroops = FindObjectsByType<Troop>(FindObjectsSortMode.None);
        foreach (Troop t in allTroops)
        {
            if (t == null || t.currentCell == null) continue;
            if (t.owner == this) myTroops.Add(t);
            else enemyTroops.Add(t);
        }

        _blackboard.Set("EnemyTroops", enemyTroops);
    }

    // Método auxiliar temporal del prototipo (solo mira los 6 hexágonos adyacentes)
    private List<HexCell> GetWalkableNeighbors(Troop troop)
    {
        List<HexCell> valid = new List<HexCell>();
        if (gridManager == null || troop == null || troop.currentCell == null) return valid;

        for (int i = 0; i < 6; i++)
        {
            HexCoordinates neighborCoords = troop.currentCell.Coordinates.GetNeighbor(i);
            HexCell cell = gridManager.GetCell(neighborCoords);

            if (cell == null || cell.IsOccupied) continue;
            if (cell.TerrainType == HexTerrainType.TerrenoAgujeroDeGusano) continue;

            bool impassable = false;
            if (troop.impassableTerrains != null)
            {
                foreach (var badTerrain in troop.impassableTerrains)
                {
                    if (cell.TerrainType == badTerrain) { impassable = true; break; }
                }
            }
            if (!impassable) valid.Add(cell);
        }
        return valid;
    }

    // -------------------------------------------------------------------------
    // Ayuda para el futuro dickstra.  Metodo original que delega la mejor casilla a Dijkstra
    // Al Descomentar el prototipo, Dijkstra decidira directamente la casilla optima
    // segun su calculo interno de prioridades e influencia.
    // 
    
    
    /*private HexCell ChooseBestCellDijkstra(Troop troop)        //BORRAR ESTO
    {
        HexCoordinates bestCoords = PathfindingDijkstra.GetBestPosition(gridManager, troop);
        return gridManager.GetCell(bestCoords);
    }*/
    

    // =========================================
    // CONDICIONES Y ACCIONES: CONSTRUCCION
    // =========================================

    private bool CanAffordTroop()
    {
        if (troopPrefab == null) return false;
        return chatarra >= troopPrefab.chatarra &&
               creditos >= troopPrefab.creditos &&
               tritio >= troopPrefab.tritio &&
               cristal >= troopPrefab.cristales;
    }

    private bool FindSpawnCell()
    {
        HexCell[] allCells = FindObjectsByType<HexCell>(FindObjectsSortMode.None);
        HexCell bestCell = null;
        float bestScore = float.MinValue;

        foreach (HexCell cell in allCells)
        {
            if (cell == null || cell.IsOccupied) continue;
            if (cell.TerrainType == HexTerrainType.TerrenoAgujeroDeGusano) continue;

            bool blocked = false;
            if (troopPrefab.impassableTerrains != null)
            {
                foreach (var t in troopPrefab.impassableTerrains)
                {
                    if (cell.TerrainType == t) { blocked = true; break; }
                }
            }
            if (blocked) continue;

            float score = 0f;
            foreach (Troop ally in myTroops)
            {
                int dist = cell.Coordinates.DistanceTo(ally.currentCell.Coordinates);
                if (dist == 1) score += 10f;
                else score -= dist;
            }

            if (score > bestScore)
            {
                bestScore = score;
                bestCell = cell;
            }
        }

        if (bestCell == null) return false;
        _blackboard.Set("SpawnCell", bestCell);
        return true;
    }

    private NodeState BuildTroopAction()
    {
        HexCell spawnCell = _blackboard.Get<HexCell>("SpawnCell");
        if (spawnCell == null || troopPrefab == null) return NodeState.Failure;

        Troop newTroop = Instantiate(troopPrefab);
        if (newTroop.Initialize(spawnCell, this))
        {
            //newTroop.ApplyFactionSkin(this.faction);
            newTroop.ChangeSkin();

            chatarra -= troopPrefab.chatarra;
            creditos -= troopPrefab.creditos;
            tritio -= troopPrefab.tritio;
            cristal -= troopPrefab.cristales;

            /* if (gameManager != null)
            {
                gameManager.ia_chatarra = chatarra;
                gameManager.ia_creditos = creditos;
                gameManager.ia_tritio = tritio;
                gameManager.ia_cristal = cristal;
            } */

            myTroops.Add(newTroop);
            Debug.Log($"IA construye {newTroop.name} en {spawnCell.Coordinates}");
            return NodeState.Success;
        }

        return NodeState.Failure;
    }

    // =========================================
    // CONDICIONES Y ACCIONES: MOVIMIENTO
    // =========================================

    private bool IsCurrentTroopLowHealth()
    {
        Troop troop = _blackboard.Get<Troop>("CurrentTroop");
        return troop != null && troop.hp < (troop.maxhp * 0.3f);
    }

    private NodeState RefreshTacticalMapsAction()
    {
        Troop troop = _blackboard.Get<Troop>("CurrentTroop");
        if (influenceManager != null && troop != null)
        {
            influenceManager.RefreshMaps(this, troop);
        }
        return NodeState.Success;
    }

    private bool FindRetreatCell()
    {
        Troop troop = _blackboard.Get<Troop>("CurrentTroop");
        if (troop == null) return false;

        // Prototipo de More de los arboles de comportamiento
        //List<HexCell> candidateCells = GetWalkableNeighbors(troop);

        // Idea de chaty para futuro dikstra -> Obtener todas las casillas alcanzables segun troop.velocity
        // Cuando PathfindingDijkstra esta listo, borraremos la linea de GetWalkableNeighbors de arriba
        // este bloque de chaty es para que la nave huya usando todo su rango de movimiento:

        
        List<HexCell> candidateCells = new List<HexCell>();
        List<HexCoordinates> reachableCoords = PathfindingDijkstra.GetValidPositions(gridManager, troop, influenceManager);
        foreach (HexCoordinates coords in reachableCoords)
        {
            HexCell cell = gridManager.GetCell(coords);
            if (cell != null && !cell.IsOccupied) candidateCells.Add(cell);
        }
        

        if (candidateCells.Count == 0) return false;

        List<Troop> enemies = _blackboard.Get<List<Troop>>("EnemyTroops");
        HexCell safestCell = null;
        float bestSafety = float.MinValue;

        foreach (HexCell cell in candidateCells)
        {
            float safety = 0f;
            if (influenceManager != null)
            {
                safety = influenceManager.SecurityMap.GetInfluence(cell.Coordinates);
            }
            else if (enemies != null)
            {
                foreach (Troop e in enemies)
                    safety += cell.Coordinates.DistanceTo(e.currentCell.Coordinates);
            }

            if (safety > bestSafety)
            {
                bestSafety = safety;
                safestCell = cell;
            }
        }

        if (safestCell == null) return false;
        _blackboard.Set("MoveDestination", safestCell);
        return true;
    }

    private bool NoEnemyInAttackRange()
    {
        Troop troop = _blackboard.Get<Troop>("CurrentTroop");
        List<Troop> enemies = _blackboard.Get<List<Troop>>("EnemyTroops");
        if (troop == null || enemies == null) return true;

        foreach (Troop enemy in enemies)
        {
            int dist = troop.currentCell.Coordinates.DistanceTo(enemy.currentCell.Coordinates);
            if (dist <= troop.range) return false;
        }
        return true;
    }

    private bool FindAdvanceCellTowardEnemy()
    {
        Troop troop = _blackboard.Get<Troop>("CurrentTroop");
        List<Troop> enemies = _blackboard.Get<List<Troop>>("EnemyTroops");
        if (troop == null || enemies == null || enemies.Count == 0) return false;

        // Buscar el enemigo más cercano
        Troop closestEnemy = null;
        int minDist = int.MaxValue;
        foreach (Troop enemy in enemies)
        {
            int d = troop.currentCell.Coordinates.DistanceTo(enemy.currentCell.Coordinates);
            if (d < minDist)
            {
                minDist = d;
                closestEnemy = enemy;
            }
        }

        if (closestEnemy == null) return false;

        // Prototipo de More a 1 casilla dyacente
       // List<HexCell> candidateCells = GetWalkableNeighbors(troop);

        // Lo que deberia hacer aqui el DIjstra es evaluar todas las casillas alcanzables con su velocidad
        
        // Chaty me dio otra opcion por si furulase, El BT filtra todas las casillas validas que devuelve Dijstra
        // y elige la que mas acerca la nave al enemigo

        
        List<HexCell> candidateCells = new List<HexCell>();
        List<HexCoordinates> reachableCoords = PathfindingDijkstra.GetValidPositions(gridManager, troop, influenceManager);
        foreach (HexCoordinates coords in reachableCoords)
        {
            HexCell cell = gridManager.GetCell(coords);
            if (cell != null && !cell.IsOccupied) candidateCells.Add(cell);
        }
                //Por si os ayudase :sob: 

        

        HexCell bestCell = null;
        int bestNewDist = minDist;

        foreach (HexCell cell in candidateCells)
        {
            int d = cell.Coordinates.DistanceTo(closestEnemy.currentCell.Coordinates);
            if (d < bestNewDist)
            {
                bestNewDist = d;
                bestCell = cell;
            }
        }

        if (bestCell == null) return false;
        _blackboard.Set("MoveDestination", bestCell);
        return true;
    }

    private NodeState MoveCurrentTroopAction()
    {
        Troop troop = _blackboard.Get<Troop>("CurrentTroop");
        HexCell destination = _blackboard.Get<HexCell>("MoveDestination");
        if (troop == null || destination == null) return NodeState.Failure;

        //  Prototipo de More
       // troop.MoveToCell(destination);

        // Para Dijstra
        // Cuando actives Dijkstra arriba, comentamos la linea de arriba que esta de forma temporal.

        // Esta carga de Troop.cs puede dar por valido el destino usando PathfindingDijkstra.GetValidPositions(): Me la dio chaty por si sirve de ayuda.
         troop.MoveToCell(destination, gridManager);

        return NodeState.Success;
    }

    // =========================================
    // CONDICIONES Y ACCIONES: ATAQUE
    // =========================================

    private bool FindLethalTargetInRange()
    {
        Troop troop = _blackboard.Get<Troop>("CurrentTroop");
        List<Troop> enemies = _blackboard.Get<List<Troop>>("EnemyTroops");
        if (troop == null || enemies == null) return false;

        foreach (Troop enemy in enemies)
        {
            int dist = troop.currentCell.Coordinates.DistanceTo(enemy.currentCell.Coordinates);
            if (dist <= troop.range && troop.CalculateDamageAgainst(enemy) >= enemy.hp)
            {
                _blackboard.Set("TargetEnemy", enemy);
                return true;
            }
        }
        return false;
    }

    private bool FindBestTargetInRange()
    {
        Troop troop = _blackboard.Get<Troop>("CurrentTroop");
        List<Troop> enemies = _blackboard.Get<List<Troop>>("EnemyTroops");
        if (troop == null || enemies == null) return false;

        Troop bestTarget = null;
        int highestDamage = -1;

        foreach (Troop enemy in enemies)
        {
            int dist = troop.currentCell.Coordinates.DistanceTo(enemy.currentCell.Coordinates);
            if (dist <= troop.range)
            {
                int dmg = troop.CalculateDamageAgainst(enemy);
                if (dmg > highestDamage)
                {
                    highestDamage = dmg;
                    bestTarget = enemy;
                }
            }
        }

        if (bestTarget == null) return false;
        _blackboard.Set("TargetEnemy", bestTarget);
        return true;
    }

    private NodeState AttackTargetAction()
    {
        Troop troop = _blackboard.Get<Troop>("CurrentTroop");
        Troop target = _blackboard.Get<Troop>("TargetEnemy");
        if (troop == null || target == null) return NodeState.Failure;

        troop.Attack(target);
        return NodeState.Success;
    }
}
