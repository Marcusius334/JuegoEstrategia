using System.Collections.Generic;
using UnityEngine;


public class AIPlayer : Player
{
    // Estructuras internas para la planificación de la IA Global
    private enum GlobalRole
    {
        Retirada,
        CazarObjetivo,
        ConquistarRecurso
    }

    private class TroopAssignment
    {
        public GlobalRole Role;
        public HexCoordinates TargetCoordinates;
        public Troop TargetEnemy;
        public Structure TargetStructure;
    }

    [Header("Configuración")]
    public Troop troopPrefab;
    // Lista opcional para asignar en el Inspector las 7 naves y que la IA elija según recursos y ventaja
    public List<Troop> availableTroopPrefabs = new List<Troop>();
    [SerializeField] private HexGridManager gridManager;
    [SerializeField] private TacticalInfluenceManager influenceManager;

    [Header("Tropas Activas")]
    public List<Troop> myTroops = new List<Troop>();

    // Arquitectura de Árboles de Comportamiento
    private Blackboard _blackboard;
    private BTNode _constructionTree;
    private BTNode _movementTree;
    private BTNode _attackTree;
    private HashSet<Structure> estructurasReservadas = new HashSet<Structure>();

    // Memoria de decisiones de la IA Global para el turno actual
    private readonly Dictionary<Troop, TroopAssignment> _globalPlan = new Dictionary<Troop, TroopAssignment>();
    private readonly Dictionary<ResourceType, float> _resourceWeights = new Dictionary<ResourceType, float>();
    private Troop _globalFocusEnemy;

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
            // Rama 1: Retirada si tiene menos del 30% de vida
            new Sequence(new List<BTNode>
            {
                new ConditionNode(IsCurrentTroopLowHealth),
                new AlwaysSucceed(new ActionNode(RefreshTacticalMapsAction)),
                new ConditionNode(FindRetreatCell),
                new ActionNode(MoveCurrentTroopAction)
            }),

            // Rama 2 (IA Global): Cazar nave enemiga prioritaria / baja de vida con A*
            new Sequence(new List<BTNode>
            {
                new ConditionNode(HasGlobalHuntAssignment),
                new ConditionNode(NoEnemyInAttackRange),
                new ConditionNode(FindGlobalHuntStepWithAStar),
                new ActionNode(MoveCurrentTroopAction)
            }),

            // Rama 3 (IA Global): Trazar rumbo hacia la estructura más rentable con A*
            new Sequence(new List<BTNode>
            {
                new ConditionNode(HasGlobalStructureAssignment),
                new ConditionNode(FindGlobalStructureStepWithAStar),
                new ActionNode(MoveCurrentTroopAction)
            }),

            // Rama 2: Buscar una posición favorable para combatir
            new Sequence(new List<BTNode>
            {
                new ConditionNode(FindAdvanceCellTowardEnemy),
                new ActionNode(MoveCurrentTroopAction)
            }),

            // Rama 3: Buscar una estructura que conquistar
            new Sequence(new List<BTNode>
            {
                new ConditionNode(FindBestStructureToCapture),
                new ActionNode(MoveCurrentTroopAction)
            }),

            // Rama 4: Mantener posición
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
            // Rama 2 (IA Global): Priorizar enemigos heridos o marcados como foco global
            new Sequence(new List<BTNode>
            {
                new ConditionNode(FindLowHealthOrFocusTargetInRange),
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
        // Analizamos la economía y el estado global del mapa antes de decidir qué construir
        EvaluarEstrategiaGlobal();
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
        return gridManager.GetCell(PathfindingAStar.GetBestPosition(gridManager, troop, troop.currentCell.Coordinates, influenceManager));  //cambiar Pathfinding por A* y en el parentesis falta la variable que será la cordenada de donde esta la tropa
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
        // Planificamos los objetivos globales (recursos rentables y enemigos débiles) para toda la flota
        EvaluarEstrategiaGlobal();

        Debug.Log("IA: Número de tropas: " + myTroops.Count);

        estructurasReservadas.Clear();
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
    // CEREBRO DE LA IA GLOBAL (ANÁLISIS DEL MAPA)
    // =========================================

    private void EvaluarEstrategiaGlobal()
    {
        _globalPlan.Clear();
        _globalFocusEnemy = null;

        if (gridManager == null) gridManager = FindFirstObjectByType<HexGridManager>();
        if (influenceManager == null) influenceManager = FindFirstObjectByType<TacticalInfluenceManager>();

        List<Troop> enemies = _blackboard.Get<List<Troop>>("EnemyTroops") ?? new List<Troop>();
        Structure[] allStructures = FindObjectsByType<Structure>(FindObjectsSortMode.None);

        // 1. Calcular ingresos actuales por turno de las estructuras controladas por la IA
        Dictionary<ResourceType, int> incomePerTurn = new Dictionary<ResourceType, int>
        {
            { ResourceType.Chatarra, 0 },
            { ResourceType.Creditos, 0 },
            { ResourceType.Tritio, 0 },
            { ResourceType.CristalKyber, 0 }
        };

        List<Structure> targetableStructures = new List<Structure>();

        foreach (Structure st in allStructures)
        {
            if (st == null) continue;
            st.ActualizarControl();

            if (st.ControllingPLayer == this)
            {
                incomePerTurn[st.ResourceProduced] += st.recursosPorTurno;
            }
            else
            {
                targetableStructures.Add(st);
            }
        }

        // 2. Calcular qué recurso es más rentable conquistar según reservas e ingresos
        CalcularRentabilidadRecursos(incomePerTurn);

        // 3. Identificar la nave enemiga más prioritaria (baja de vida o amenazando estructuras)
        float bestEnemyPriority = float.MinValue;
        foreach (Troop enemy in enemies)
        {
            if (enemy == null || enemy.currentCell == null) continue;

            float hpRatio = (float)enemy.hp / Mathf.Max(1, enemy.maxhp);
            float threatValue = enemy.attack + (enemy.maxhp * 0.25f);

            // Gran prioridad si el enemigo tiene menos del 50% de vida
            if (hpRatio <= 0.5f) threatValue += (1f - hpRatio) * 800f;

            // Prioridad extra si está intentando capturar una estructura nuestra
            for (int i = 0; i < 6; i++)
            {
                HexCell neighbor = gridManager.GetCell(enemy.currentCell.Coordinates.GetNeighbor(i));
                if (neighbor != null && neighbor.HasStructure && neighbor.OccupyingStructure.ControllingPLayer == this)
                {
                    threatValue += 350f;
                    break;
                }
            }

            if (threatValue > bestEnemyPriority)
            {
                bestEnemyPriority = threatValue;
                _globalFocusEnemy = enemy;
            }
        }

        // 4. Puntuar cada estructura libre o enemiga según la rentabilidad de su recurso
        Dictionary<Structure, float> structureScores = new Dictionary<Structure, float>();
        foreach (Structure st in targetableStructures)
        {
            HexCell stCell = st.GetComponentInParent<HexCell>();
            if (stCell == null) continue;

            float resourceWeight = _resourceWeights[st.ResourceProduced];
            float score = (st.recursosPorTurno * resourceWeight) + (st.StrategicValue * 5f);

            // Penalizar si hay defensores enemigos alrededor
            int defenders = 0;
            for (int i = 0; i < 6; i++)
            {
                HexCell adj = gridManager.GetCell(stCell.Coordinates.GetNeighbor(i));
                if (adj != null && adj.OccupyingTroop != null && adj.OccupyingTroop.owner != this)
                    defenders++;
            }
            score -= defenders * 120f;

            structureScores[st] = score;
        }

        // 5. Repartir misiones entre las naves de la IA sin duplicar estructuras objetivo
        HashSet<Structure> globalReservedStructures = new HashSet<Structure>();

        foreach (Troop troop in myTroops)
        {
            if (troop == null || troop.currentCell == null) continue;

            if (troop.hp < troop.maxhp * 0.3f)
            {
                _globalPlan[troop] = new TroopAssignment { Role = GlobalRole.Retirada };
                continue;
            }

            Structure bestSt = null;
            HexCell bestStAdjCell = null;
            float bestStNetScore = float.MinValue;

            foreach (var kvp in structureScores)
            {
                Structure st = kvp.Key;
                if (globalReservedStructures.Contains(st)) continue;

                HexCell adjCell = GetBestAdjacentFreeCellForStructure(st, troop);
                if (adjCell == null) continue;

                int dist = troop.currentCell.Coordinates.DistanceTo(adjCell.Coordinates);
                float netScore = kvp.Value - (dist * 35f);

                if (netScore > bestStNetScore)
                {
                    bestStNetScore = netScore;
                    bestSt = st;
                    bestStAdjCell = adjCell;
                }
            }

            Troop bestHuntTarget = null;
            float bestHuntScore = float.MinValue;

            foreach (Troop enemy in enemies)
            {
                if (enemy == null || enemy.currentCell == null) continue;
                int dist = troop.currentCell.Coordinates.DistanceTo(enemy.currentCell.Coordinates);
                float hpRatio = (float)enemy.hp / Mathf.Max(1, enemy.maxhp);
                int dmg = troop.CalculateDamageAgainst(enemy);

                float huntScore = dmg * 1.5f - (dist * 45f);
                if (hpRatio <= 0.45f) huntScore += 450f;
                if (enemy == _globalFocusEnemy) huntScore += 200f;

                if (huntScore > bestHuntScore)
                {
                    bestHuntScore = huntScore;
                    bestHuntTarget = enemy;
                }
            }

            if (bestHuntTarget != null && (bestSt == null || bestHuntScore > bestStNetScore))
            {
                _globalPlan[troop] = new TroopAssignment
                {
                    Role = GlobalRole.CazarObjetivo,
                    TargetEnemy = bestHuntTarget,
                    TargetCoordinates = bestHuntTarget.currentCell.Coordinates
                };
            }
            else if (bestSt != null && bestStAdjCell != null)
            {
                globalReservedStructures.Add(bestSt);
                _globalPlan[troop] = new TroopAssignment
                {
                    Role = GlobalRole.ConquistarRecurso,
                    TargetStructure = bestSt,
                    TargetCoordinates = bestStAdjCell.Coordinates
                };
            }
        }
    }

    private void CalcularRentabilidadRecursos(Dictionary<ResourceType, int> incomePerTurn)
    {
        // Valoramos más los recursos escasos o de los que aún no tenemos producción por turno
        _resourceWeights[ResourceType.Chatarra] = (chatarra < 600 ? 25f : 12f) + (incomePerTurn[ResourceType.Chatarra] == 0 ? 15f : 0f);
        _resourceWeights[ResourceType.Creditos] = (creditos < 300 ? 22f : 10f) + (incomePerTurn[ResourceType.Creditos] == 0 ? 15f : 0f);
        _resourceWeights[ResourceType.Tritio] = (tritio < 60 ? 24f : 11f) + (incomePerTurn[ResourceType.Tritio] == 0 ? 18f : 0f);
        _resourceWeights[ResourceType.CristalKyber] = (cristal < 2 ? 35f : 14f) + (incomePerTurn[ResourceType.CristalKyber] == 0 ? 20f : 0f);

        // Si ya tenemos chatarra y créditos suficientes, priorizamos Kyber y Tritio para naves avanzadas
        if (chatarra >= 1200 && creditos >= 500)
        {
            _resourceWeights[ResourceType.CristalKyber] += 25f;
            _resourceWeights[ResourceType.Tritio] += 15f;
        }
    }

    private HexCell GetBestAdjacentFreeCellForStructure(Structure structure, Troop troop)
    {
        HexCell stCell = structure.GetComponentInParent<HexCell>();
        if (stCell == null || gridManager == null) return null;

        HexCell bestCell = null;
        int minDist = int.MaxValue;

        for (int i = 0; i < 6; i++)
        {
            HexCoordinates adjCoords = stCell.Coordinates.GetNeighbor(i);
            HexCell adjCell = gridManager.GetCell(adjCoords);
            if (adjCell == null) continue;

            // Si la nave ya está junto a la estructura, mantenemos esa casilla como objetivo
            if (adjCell == troop.currentCell) return adjCell;

            if (adjCell.IsOccupied || adjCell.HasStructure) continue;
            if (adjCell.TerrainType == HexTerrainType.TerrenoAgujeroDeGusano) continue;

            int d = troop.currentCell.Coordinates.DistanceTo(adjCoords);
            if (d < minDist)
            {
                minDist = d;
                bestCell = adjCell;
            }
        }
        return bestCell;
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
        // Si hay lista de naves disponibles en el Inspector, elegimos la mejor que podamos pagar
        if (availableTroopPrefabs != null && availableTroopPrefabs.Count > 0)
        {
            Troop bestPrefab = null;
            float bestPower = float.MinValue;

            foreach (Troop prefab in availableTroopPrefabs)
            {
                if (prefab == null) continue;
                bool affordable = chatarra >= prefab.chatarra &&
                                  creditos >= prefab.creditos &&
                                  tritio >= prefab.tritio &&
                                  cristal >= prefab.cristales;
                if (!affordable) continue;

                float power = prefab.attack * 2f + prefab.hp * 0.5f + prefab.velocity * 20f;
                if (power > bestPower)
                {
                    bestPower = power;
                    bestPrefab = prefab;
                }
            }

            if (bestPrefab != null)
            {
                _blackboard.Set("SelectedTroopPrefab", bestPrefab);
                return true;
            }
        }

        if (troopPrefab == null) return false;
        bool canAffordDefault = chatarra >= troopPrefab.chatarra &&
                                creditos >= troopPrefab.creditos &&
                                tritio >= troopPrefab.tritio &&
                                cristal >= troopPrefab.cristales;

        if (canAffordDefault)
        {
            _blackboard.Set("SelectedTroopPrefab", troopPrefab);
        }
        return canAffordDefault;
    }

    private bool FindSpawnCell()
    {
        // Usamos el prefab elegido por CanAffordTroop (o troopPrefab por defecto)
        Troop prefabToUse = _blackboard.Get<Troop>("SelectedTroopPrefab") ?? troopPrefab;
        if (prefabToUse == null) return false;

        HexCell[] allCells = FindObjectsByType<HexCell>(FindObjectsSortMode.None);
        HexCell bestCell = null;
        float bestScore = float.MinValue;

        // Comprobamos si la nave requiere una estructura adyacente para poder desplegarse
        bool requiresStructure = prefabToUse.estructurasConstruibles != null && prefabToUse.estructurasConstruibles.Length > 0;

        foreach (HexCell cell in allCells)
        {
            if (cell == null || cell.IsOccupied || cell.HasStructure) continue;
            if (cell.TerrainType == HexTerrainType.TerrenoAgujeroDeGusano) continue;

            bool blocked = false;
            if (prefabToUse.impassableTerrains != null)
            {
                foreach (var t in prefabToUse.impassableTerrains)
                {
                    if (cell.TerrainType == t) { blocked = true; break; }
                }
            }
            if (blocked) continue;

            if (requiresStructure)
            {
                bool hasValidStructure = false;
                foreach (StructureTpe stType in prefabToUse.estructurasConstruibles)
                {
                    if (cell.ComprobarEstructuraCercana(stType))
                    {
                        hasValidStructure = true;
                        break;
                    }
                }
                if (!hasValidStructure) continue;
            }

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
        Troop prefabToBuild = _blackboard.Get<Troop>("SelectedTroopPrefab") ?? troopPrefab;
        if (spawnCell == null || prefabToBuild == null) return NodeState.Failure;

        Troop newTroop = Instantiate(prefabToBuild);
        // newTroop.Initialize() ya descuenta automáticamente creditos, chatarra, tritio y cristal al owner
        if (newTroop.Initialize(spawnCell, this))
        {
            newTroop.ApplyFactionSkin(this.faction);

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

        // Trazamos la ruta de retirada con A* hacia la casilla segura elegida
        HexCoordinates aStarCoords = PathfindingAStar.GetBestPosition(gridManager, troop, safestCell.Coordinates, influenceManager);
        HexCell aStarCell = gridManager.GetCell(aStarCoords);

        _blackboard.Set("MoveDestination", (aStarCell != null && !aStarCell.IsOccupied) ? aStarCell : safestCell);
        return true;
    }

    // Condiciones y acciones de ruta con A* guiadas por la IA Global
    private bool HasGlobalHuntAssignment()
    {
        Troop troop = _blackboard.Get<Troop>("CurrentTroop");
        if (troop == null || !_globalPlan.TryGetValue(troop, out var plan)) return false;
        return plan.Role == GlobalRole.CazarObjetivo && plan.TargetEnemy != null && plan.TargetEnemy.currentCell != null;
    }

    private bool FindGlobalHuntStepWithAStar()
    {
        Troop troop = _blackboard.Get<Troop>("CurrentTroop");
        if (troop == null || !_globalPlan.TryGetValue(troop, out var plan)) return false;

        if (influenceManager != null) influenceManager.RefreshMaps(this, troop);

        HexCoordinates goal = plan.TargetEnemy.currentCell.Coordinates;
        HexCoordinates stepCoords = PathfindingAStar.GetBestPosition(gridManager, troop, goal, influenceManager);
        HexCell stepCell = gridManager.GetCell(stepCoords);

        if (stepCell == null || stepCell == troop.currentCell || stepCell.IsOccupied) return false;

        _blackboard.Set("MoveDestination", stepCell);
        Debug.Log($"IA Global (A*): {troop.name} persigue a {plan.TargetEnemy.name} -> paso a {stepCell.Coordinates}");
        return true;
    }

    private bool HasGlobalStructureAssignment()
    {
        Troop troop = _blackboard.Get<Troop>("CurrentTroop");
        if (troop == null || !_globalPlan.TryGetValue(troop, out var plan)) return false;
        return plan.Role == GlobalRole.ConquistarRecurso && plan.TargetStructure != null;
    }

    private bool FindGlobalStructureStepWithAStar()
    {
        Troop troop = _blackboard.Get<Troop>("CurrentTroop");
        if (troop == null || !_globalPlan.TryGetValue(troop, out var plan)) return false;

        // Si ya está adyacente a la estructura asignada, mantiene la posición para controlarla
        if (troop.currentCell.Coordinates.Equals(plan.TargetCoordinates)) return false;

        if (influenceManager != null) influenceManager.RefreshMaps(this, troop);

        HexCoordinates stepCoords = PathfindingAStar.GetBestPosition(gridManager, troop, plan.TargetCoordinates, influenceManager);
        HexCell stepCell = gridManager.GetCell(stepCoords);

        if (stepCell == null || stepCell == troop.currentCell || stepCell.IsOccupied) return false;

        _blackboard.Set("MoveDestination", stepCell);
        Debug.Log($"IA Global (A*): {troop.name} rumbo a {plan.TargetStructure.StructureType} ({plan.TargetStructure.ResourceProduced}) -> paso a {stepCell.Coordinates}");
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

    /* El que hizo more, lo dejo por si acaso era necesario, pero el que voy a poner se adapta al arbol local y se moveran al más eficiente
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
    */


    private bool FindAdvanceCellTowardEnemy()
    {
        Troop troop = _blackboard.Get<Troop>("CurrentTroop");

        if (troop == null || troop.currentCell == null)
            return false;

        // Seleccionar el enemigo más interesante.
        if (!FindBestEnemyToEngage())
            return false;

        Troop target = _blackboard.Get<Troop>("TargetEnemy");

        if (target == null || target.currentCell == null)
            return false;

        // Usamos A* para trazar el camino hacia la posición del objetivo enemigo
        HexCoordinates stepCoords = PathfindingAStar.GetBestPosition(
            gridManager, troop, target.currentCell.Coordinates, influenceManager
        );
        HexCell bestCell = gridManager.GetCell(stepCoords);

        // Si no encontramos una posición mejor, se queda donde está.
        if (bestCell == null || bestCell == troop.currentCell || bestCell.IsOccupied)
            return false;

        _blackboard.Set("MoveDestination", bestCell);

        Debug.Log(
            $"{troop.name} busca posicionarse frente a {target.name} " +
            $"a distancia {bestCell.Coordinates.DistanceTo(target.currentCell.Coordinates)}"
        );

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

        if (troop == null || troop.currentCell == null ||
            enemies == null || enemies.Count == 0)
            return false;

        Troop bestTarget = null;
        float bestScore = float.MinValue;

        foreach (Troop enemy in enemies)
        {
            if (enemy == null || enemy.currentCell == null)
                continue;

            int distance = troop.currentCell.Coordinates.DistanceTo(
                enemy.currentCell.Coordinates
            );

            if (distance > troop.range)
                continue;

            int damage = troop.CalculateDamageAgainst(enemy);

            // Solo nos interesan los enemigos que podemos eliminar.
            if (damage < enemy.hp)
                continue;

            float score = CalculateEnemyScore(troop, enemy);

            if (score > bestScore)
            {
                bestScore = score;
                bestTarget = enemy;
            }
        }

        if (bestTarget == null)
            return false;

        _blackboard.Set("TargetEnemy", bestTarget);

        Debug.Log(
            $"{troop.name} puede eliminar a {bestTarget.name}. " +
            $"Puntuación: {bestScore}"
        );

        return true;
    }

    // Prioriza atacar a enemigos heridos (<= 50% HP) o al objetivo global marcado
    private bool FindLowHealthOrFocusTargetInRange()
    {
        Troop troop = _blackboard.Get<Troop>("CurrentTroop");
        List<Troop> enemies = _blackboard.Get<List<Troop>>("EnemyTroops");

        if (troop == null || troop.currentCell == null ||
            enemies == null || enemies.Count == 0)
            return false;

        Troop bestTarget = null;
        float lowestHpRatio = 0.50f;

        foreach (Troop enemy in enemies)
        {
            if (enemy == null || enemy.currentCell == null)
                continue;

            int distance = troop.currentCell.Coordinates.DistanceTo(
                enemy.currentCell.Coordinates
            );

            if (distance > troop.range)
                continue;

            float hpRatio = (float)enemy.hp / Mathf.Max(1, enemy.maxhp);
            if (enemy == _globalFocusEnemy || hpRatio <= lowestHpRatio)
            {
                lowestHpRatio = hpRatio;
                bestTarget = enemy;
            }
        }

        if (bestTarget == null)
            return false;

        _blackboard.Set("TargetEnemy", bestTarget);
        return true;
    }

    private bool FindBestTargetInRange()
    {
        Troop troop = _blackboard.Get<Troop>("CurrentTroop");
        List<Troop> enemies = _blackboard.Get<List<Troop>>("EnemyTroops");

        if (troop == null || troop.currentCell == null ||
            enemies == null || enemies.Count == 0)
            return false;

        Troop bestTarget = null;
        float bestScore = float.MinValue;

        foreach (Troop enemy in enemies)
        {
            if (enemy == null || enemy.currentCell == null)
                continue;

            int distance = troop.currentCell.Coordinates.DistanceTo(
                enemy.currentCell.Coordinates
            );

            if (distance > troop.range)
                continue;

            float score = CalculateEnemyScore(troop, enemy);

            if (score > bestScore)
            {
                bestScore = score;
                bestTarget = enemy;
            }
        }

        if (bestTarget == null)
            return false;

        _blackboard.Set("TargetEnemy", bestTarget);

        Debug.Log(
            $"{troop.name} ataca a {bestTarget.name}. " +
            $"Puntuación: {bestScore}"
        );

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

    //-----------------------------------------------------------------------------
    //arbol local
    //-----------------------------------------------------------------------------

    //enemigos con mas o menos importancia
    //se mirara si es efectivo, si esta más cerca, si esta debil, etc...
    
    private float CalculateEnemyScore(Troop troop, Troop enemy)
    {
        if (troop == null || enemy == null ||
            troop.currentCell == null || enemy.currentCell == null)
            return float.MinValue;

        int damageDealt = troop.CalculateDamageAgainst(enemy);

        int distance = troop.currentCell.Coordinates.DistanceTo(
            enemy.currentCell.Coordinates
        );

        // Solo considerar el daño recibido si el enemigo
        // tiene alcance suficiente para atacarnos.
        int damageReceived = 0;

        if (distance <= enemy.range)
        {
            damageReceived = enemy.CalculateDamageAgainst(troop);
        }

        float score = 0f;

        // Daño que podemos causar.
        score += damageDealt;

        // Priorizar objetivos que podemos eliminar.
        if (damageDealt >= enemy.hp)
            score += 500f;

        // Bonificación adicional si el enemigo ya está herido o es el objetivo global
        float hpRatio = (float)enemy.hp / Mathf.Max(1, enemy.maxhp);
        score += (1f - hpRatio) * 300f;
        if (enemy == _globalFocusEnemy)
            score += 200f;

        // Penalizar el peligro real del enfrentamiento.
        score -= damageReceived * 0.5f;

        // Penalizar enemigos demasiado lejanos.
        score -= distance * 50f;

        return score;
    }



    //mirar cual es el que tiene más puntuacion 
    private bool FindBestEnemyToEngage()
    {
        Troop troop = _blackboard.Get<Troop>("CurrentTroop");
        List<Troop> enemies = _blackboard.Get<List<Troop>>("EnemyTroops");

        if (troop == null || enemies == null || enemies.Count == 0)
            return false;

        Troop bestEnemy = null;
        float bestScore = float.MinValue;

        foreach (Troop enemy in enemies)
        {
            if (enemy == null || enemy.currentCell == null)
                continue;

            float score = CalculateEnemyScore(troop, enemy);

            if (score > bestScore)
            {
                bestScore = score;
                bestEnemy = enemy;
            }
        }

        if (bestEnemy == null)
            return false;

        _blackboard.Set("TargetEnemy", bestEnemy);

        Debug.Log(
            $"{troop.name} selecciona como objetivo a {bestEnemy.name}. " +
            $"Puntuación: {bestScore}"
        );

        return true;
    }


    private float CalculatePositionScore(HexCell cell, Troop target, Troop troop)
    {
        if (cell == null || target == null || troop == null)
            return float.MinValue;

        int distance = cell.Coordinates.DistanceTo(
            target.currentCell.Coordinates
        );

        int desiredRange = troop.range;

        // Para estar en rango max
        if (distance <= desiredRange)
        {
            return 1000f - (desiredRange - distance) * 100f;
        }

        // Si estamos fuera de rango, se pone más cercaa
        return -(distance - desiredRange) * 100f;
    }

    private bool FindBestStructureToCapture()
    {
        Troop troop = _blackboard.Get<Troop>("CurrentTroop");

        if (troop == null || troop.currentCell == null)
            return false;

        // Calculamos las posiciones alcanzables una sola vez.
        List<HexCoordinates> reachable =
            PathfindingDijkstra.GetValidPositions(
                gridManager, troop, influenceManager
            );

        HexCell[] allCells = FindObjectsByType<HexCell>(
            FindObjectsSortMode.None
        );

        HexCell bestCell = null;
        Structure bestStructure = null;
        float bestScore = float.MinValue;

        foreach (HexCell structureCell in allCells)
        {
            if (structureCell == null || !structureCell.HasStructure)
                continue;

            Structure structure = structureCell.OccupyingStructure;

            if (structure == null)
                continue;

            // Ignorar estructuras que ya controla la IA.
            if (structure.ControllingPLayer == this)
                continue;

            if (estructurasReservadas.Contains(structure))
                continue;

            for (int i = 0; i < 6; i++)
            {
                HexCoordinates coords =
                    structureCell.Coordinates.GetNeighbor(i);

                HexCell adjacentCell = gridManager.GetCell(coords);

                if (adjacentCell == null || adjacentCell.IsOccupied)
                    continue;

                if (!reachable.Contains(coords))
                    continue;

                int distance =
                    troop.currentCell.Coordinates.DistanceTo(coords);

                float score =
                    structure.StrategicValue * 10f - distance * 20f;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestCell = adjacentCell;
                    bestStructure = structure;
                }
            }
        }

        if (bestCell == null)
            return false;

        _blackboard.Set("MoveDestination", bestCell);
        _blackboard.Set("TargetStructure", bestStructure);

        estructurasReservadas.Add(bestStructure);

        Debug.Log(
            $"{troop.name} intenta conquistar una estructura. " +
            $"Destino: {bestCell.Coordinates}. Puntuación: {bestScore}"
        );

        return true;
    }

}



