# Modellr

Modellr is a basic 3D modelling program created using Unity and C#.

## How to Use:
To move the view around, right-click drag (or middlemouse) to rotate the view.
Hold shift while dragging to pan.
Zoom in/out with the scroll wheel.

There are two main modes for selection/editing, **Vertex** and **Face**. These can be toggled using the dropdown at the top left.
- In **Vertex** mode, parts of the mesh are selected by vertices, then translated/rotated/scaled using the gizmo.
- In **Face** mode, parts of the mesh are selected by faces. As well as translation/rotation/scaling, **Face** mode allows the user to extrude faces using the extrude tool.

## Functions:
- Select points [vertices/faces] (Left click, Shift + Left click to select multiple)
- Select/Deselect all ([A] key)
- Extrude selected faces (Extrude tool or [E] key)
- Create a face connecting the selected vertices ('Face from Verts' button or [F] key)
- Look at selected ('Look At' button or [Home] key)
- Box select ('Box Select' button or [Alt+drag])
- Select linked vertices ('Delete Points' button or [Del] key)
- Undo/Redo (buttons or [Ctrl+Z]/[Ctrl+Y])
- Insert mesh primitives (Dropdown at top-right)
- Save/Load to or from an OBJ file
- Toggle on/off the visibility of Edges, Faces and Points (when Points visibility is off, points near the cursor will still be visible).
