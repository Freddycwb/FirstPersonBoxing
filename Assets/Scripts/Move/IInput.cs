using UnityEngine;

public interface IInput
{
    Vector3 direction { get; }
    bool attack1 { get; }
    bool attack2 { get; }
    bool defending { get; }
}