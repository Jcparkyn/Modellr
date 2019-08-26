using UnityEngine;

//Interface for selectable objects (Face and Vertex)
public interface ISelectable
{
    Vector3 Pos { get; }
    bool Selected { get; set; }
    void SelectAdditional();
    void SelectAbsolute();
}