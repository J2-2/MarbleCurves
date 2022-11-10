# Marble Curves

## User Manuel

### Create New Nodes
Make a new set of nodes with the number to create specified by __Number of Nodes__.

### Load Selected Nodes
Load the set of nodes that is selected in the heirarchy.

### Delete Node
Delete the node from the currently selected set of nodes at the position specified by __Position__.

### Add Node
Add a node to the currently selected set of nodes at the position specified by __Position__.

### Shape
__rectange__: The cross section of the curve will be a rectangle.  
__tube__: The cross section of the curve will be a circle.

### Width/Height
The width and height that the rectangle cross section of the curve will have when in __rectangle__ mode.

### Inner Radius/Outer Radius
The distance to the inside edge and outside edge of the circle cross section the curve will have when in __tube__ mode.

### Width Offset/Height Offset
The distance away from the middle of the curve that the cross section will be centered on.

### Length Step Size/Width Step Size
The size of the faces that the curve will be broken up into across it's surface.

### Round Tile to the Nearest
The uvs and faces along the length of the curve will be rounded to this value for exmple if the curve is 20.6 units long and __Round Tile to the Nearest__ is set to 1 then the curve will be rounded to 21 units long.

### UV Scale Length/Width/Height
The UV scaling along the length of the curve and the width and height of the cross section. When in __tube__ mode width is the scaling around the circumference of the circle and height is the scaling along the width of the circle.

### UV Offset Length/Width
The Offset for the UVs along the length and width of the curve.

### Node Smoothness Values
The node smoothness values determines how straight the curve will be around a particular node, the higher the node smoothness value the straighter the curve will be at that node. 

### Set All
Set the node smoothness values of every node in the selected node set to the value specified by __Smoothness Value__

### Auto-Position Internal Nodes
Set the direction and node smoothness values of every node in the selected node set except for the first and last node such that the curve is as smooth as possible.

### Angle Interpretation
__linear__: The angle of the platform between two nodes will be interpolated linearly.  
__smooth__: The anlge of the platform between two nodes will be disproportionately close to the node that it is closer to.

### Include Faces
Which faces of the curve to include in the final mesh.

### Make Curve
Create the curve object based on the currently selected nodes and the parameters selected.
