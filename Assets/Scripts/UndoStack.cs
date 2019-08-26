using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// This class is used to store the Undo/Redo stack for edits made to the mesh.
/// It uses an array internally and stores an index to the item at the top of the list,
/// to allow for items to be added/removed without moving the other items in the array
/// (making add/remove operations extremely fast, even with a very large number of steps saved).
/// The MeshEdit class uses this to store previous versions of a MeshObject (converted to
/// MeshObjectSerializable for immutability and lesser memory usage).
/// </summary>
public class UndoStack<T> {

    public T[] items { get; private set; }

    private int topIndex;
    private int undoDepth;
    private int undoMax;

    private int IncIndex(int start, int increment = 1)
    {
        int result = ((start + increment) % items.Length + items.Length) % items.Length;
        return result;
    }

    public UndoStack(int length)
    {
        items = new T[length];
        topIndex = length - 1;
        undoDepth = 0;
        undoMax = 0;
    }

    //Method called to perform an action. 
    public void Do(T item)
    {
        if (undoDepth > 0)
        {
            topIndex = IncIndex(topIndex, -undoDepth);
            undoDepth = 0;
        }
        topIndex = IncIndex(topIndex);

        if (undoMax < items.Length)
        {
            undoMax += 1;
        }
        items[topIndex] = item;
    }

    //Method called to Undo. Will return false if there are no undo steps left.
    //Returns the desired version of the object as an out variable.
    public bool TryUndo(out T item)
    {
        if (undoDepth < undoMax - 1)
        {
            undoDepth += 1;
            item = items[IncIndex(topIndex, -undoDepth)];
            return true;
        }
        item = items[IncIndex(topIndex, -undoDepth)];
        return false;
    }

    //Method called to Redo. Will return false if there are no redo steps left.
    //Returns the desired version of the object as an out variable.
    public bool TryRedo(out T item)
    {
        if (undoDepth > 0)
        {
            undoDepth -= 1;
            item = items[IncIndex(topIndex, -undoDepth)]; ;
            return true;
        }
        item = items[IncIndex(topIndex, -undoDepth)]; ;
        return false;
    }
}
