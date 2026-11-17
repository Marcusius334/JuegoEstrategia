using System;
using System.Collections.Generic;

public enum NodeState
{
    Success,
    Failure,
    Running
}

// Memoria compartida entre nodos de condición y acción
public class Blackboard
{
    private readonly Dictionary<string, object> _data = new Dictionary<string, object>();

    public void Set<T>(string key, T value) => _data[key] = value;

    public T Get<T>(string key) => _data.TryGetValue(key, out var val) ? (T)val : default;

    public void Clear() => _data.Clear();
}

public abstract class BTNode
{
    public abstract NodeState Evaluate();
}

// Selector (?): Lógica OR, evalúa de izquierda a derecha hasta que uno tenga éxito
public class Selector : BTNode
{
    private readonly List<BTNode> _children;
    public Selector(List<BTNode> children) => _children = children;

    public override NodeState Evaluate()
    {
        foreach (var child in _children)
        {
            NodeState state = child.Evaluate();
            if (state == NodeState.Running) return NodeState.Running;
            if (state == NodeState.Success) return NodeState.Success;
        }
        return NodeState.Failure;
    }
}

// Sequence (->): Lógica AND, falla en cuanto un hijo devuelve Failure
public class Sequence : BTNode
{
    private readonly List<BTNode> _children;
    public Sequence(List<BTNode> children) => _children = children;

    public override NodeState Evaluate()
    {
        foreach (var child in _children)
        {
            NodeState state = child.Evaluate();
            if (state == NodeState.Running) return NodeState.Running;
            if (state == NodeState.Failure) return NodeState.Failure;
        }
        return NodeState.Success;
    }
}

// Decorator AlwaysSucceed: Ejecuta una rama opcional sin bloquear la secuencia si falla
public class AlwaysSucceed : BTNode
{
    private readonly BTNode _child;
    public AlwaysSucceed(BTNode child) => _child = child;

    public override NodeState Evaluate()
    {
        _child.Evaluate();
        return NodeState.Success;
    }
}

// Nodo Hoja: Condition (solo consulta estado, nunca devuelve Running)
public class ConditionNode : BTNode
{
    private readonly Func<bool> _condition;
    public ConditionNode(Func<bool> condition) => _condition = condition;

    public override NodeState Evaluate() => _condition() ? NodeState.Success : NodeState.Failure;
}

// Nodo Hoja: Action (modifica el estado del juego)
public class ActionNode : BTNode
{
    private readonly Func<NodeState> _action;
    public ActionNode(Func<NodeState> action) => _action = action;

    public override NodeState Evaluate() => _action();
}
