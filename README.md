# Marble Curves

Marble Curves is a Unity package that provides a node based system for creating 3D curves. Included in this document are installation instructions, links to some video tutorials and a user manual.

## Installation
To install Marble Curves, on github go to Code>Download Zip to download a zip file containing the unity package then extract the contents. Then in Unity go to Assets>Import Package>Custom package then selected the .unitypackage file in the newly extracted files. Now an option for the Marble Curves Window should appear in the Tool tab in Unity.

## User Manual

### Table of Contents

[Nodes](#nodes)  
[Rectangle Mode](#rectangle_mode)  
[Tube Mode](#tube_mode)  
[Custom Mode](#custom_mode)  
[Fit to Curve Mode](#fit_to_curve_mode)
[UVs](#uvs)
[Curve Options](#curve_options)

<a name="nodes"></a>
### Nodes

Nodes are the basic tool used to create the shape for curves. The button __Create New Nodes__ creates a Nodes object in the heirarchy with a number of node object children specified by __Number of Nodes__. They will spawn in front of the camera in the scene. Wherever the nodes are positioned within the scene the curve will go through them. The curve will also be going in the drection of the node at that point and it's rotation will be determined by the rotation of the node. This means that all kinds of shaped curves can be made by positioning the nodes in different ways. 

![Nodes](https://github.com/J2-2/MarbleCurves/blob/main/UserManualResources/Nodes.png?raw=true)

__<ins>Buttons</ins>__

__Create New Nodes:__  
Make a new set of nodes with the number to create specified by __Number of Nodes__.

__Load Selected Nodes:__  
Load the set of nodes that is selected in the heirarchy. You can have either the nodes parent object selected or any of the node objects selected.

__Delete Node:__  
Delete the node from the currently selected set of nodes at the position specified by __Position__.

__Add Node:__  
Add a node to the currently selected set of nodes at the position specified by __Position__.

### Node Smoothness Values

Node smoothness value is a property of each node that determines how straight the curve will be at that node. The higher the smoothness value the straighter the curve. Here is an example of 3 curves with nodes in the same postion but with different smoothness values.

<table border="0"><tr>
  <td> <img src="https://github.com/J2-2/MarbleCurves/blob/main/UserManualResources/Smoothness20.png?raw=true" alt="Smoothness 20" style="width: 350px;"/> </td>
  <td> <img src="https://github.com/J2-2/MarbleCurves/blob/main/UserManualResources/Smoothness30.png?raw=true" alt="Smoothness 30" style="width: 350px;"/> </td>
  <td> <img src="https://github.com/J2-2/MarbleCurves/blob/main/UserManualResources/Smoothness50.png?raw=true" alt="Smoothness 50" style="width: 350px;"/> </td>
</tr>
<tr>
  <td>Node smoothness is 20 for all nodes</td>
  <td>Node smoothness is 30 for all nodes</td>
  <td>Node smoothness is 50 for all nodes</td>
</tr></table>

The nodes do not have to have the same smoothness value, they can be changed individually to achive a desired curve shape. If just a smooth curve is desired then the __Auto-Position Internal Nodes__ button will change the orientation and node smoothness of each node except for the ones on the end to make it as smooth as possible. This means to make a smooth curve all that needs to be done is to put nodes in the desired postion, orient the first and last nodes and choose desired node smoothness and then __Auto-Position Internal Nodes__ will do the rest.

__<ins>Buttons</ins>__

__Set All:__  
Sets the value of each node's node smoothness to the value specified by __Smoothness Value__.

__Auto-Position Internal Nodes:__  
Sets the orientation and node smoothness of each node except for the first and last one based on their positions and the direction and node smoothess of the first and lasts nodes in a way as to make the curve as smooth as possible.

<a name="rectangle_mode"></a>
### Rectangle Mode

Ractangle mode can be selected by choosing the __rectangle__ option in the mode selector. This mode makes it so that the cross section of the curve will be a rectangle with dimensions specified by __Width__ and __Height__. The middle of the curve will be centered on the top edge of the rectangle.

__<ins>Parameters</ins>__

__Width/Height:__  
These control the width and height of the cross section of the curve.

<table><tr>
  <td> <img src="https://github.com/J2-2/MarbleCurves/blob/main/UserManualResources/width8height1.png?raw=true" alt="Width 8, Height 1" style="width: 350px;"/> </td>
  <td> <img src="https://github.com/J2-2/MarbleCurves/blob/main/UserManualResources/width4height4.png?raw=true" alt="Width 4, Height 4" style="width: 350px;"/> </td>
  <td> <img src="https://github.com/J2-2/MarbleCurves/blob/main/UserManualResources/width1height2.png?raw=true" alt="Width 1, Height 2" style="width: 350px;"/> </td>
</tr>
<tr>
  <td>Width is 8 and Height is 1</td>
  <td>Width is 4 and Height is 4</td>
  <td>Width is 1 and Height is 2</td>
</tr></table>

__Width Offset/Height Offset__:
These control how offset the cross section is from the center of the curve. When set to 0 and 0 the center of the curve is positioned in the middle of the top edge of the rectangle cross section. 

<table><tr>
  <td> <img src="https://github.com/J2-2/MarbleCurves/blob/main/UserManualResources/offset2_0.png?raw=true" alt="Width Offset 2, Height Offset 0" style="width: 350px;"/> </td>
  <td> <img src="https://github.com/J2-2/MarbleCurves/blob/main/UserManualResources/offset-4_4.png?raw=true" alt="Width Offset -4, Height Offset 4" style="width: 350px;"/> </td>
  <td> <img src="https://github.com/J2-2/MarbleCurves/blob/main/UserManualResources/offset0_0.5.png?raw=true" alt="Width Offset 0, Height Offset 0.5" style="width: 350px;"/> </td>
</tr>
<tr>
  <td>Width Offset is 2 and Height Offset is 0</td>
  <td>Width Offset is -4 and Height Offset is 4</td>
  <td>Width Offset is 0 and Height Offset is 0.5</td>
</tr></table>

__Length Step Size/Width Step Size:__  
These control the dimensions of the faces that the curve will be subdivided into. The smaller the size the smoother the geometry will be but the curve will take up more memory and take longer to make. Using larger face sizes can create a jagged edge on the curve.

<table><tr>
  <td> <img src="https://github.com/J2-2/MarbleCurves/blob/main/UserManualResources/step0.5.png?raw=true" alt="Length Step 0.5" style="width: 350px;"/> </td>
  <td> <img src="https://github.com/J2-2/MarbleCurves/blob/main/UserManualResources/step1.png?raw=true" alt="Length Step 1" style="width: 350px;"/> </td>
  <td> <img src="https://github.com/J2-2/MarbleCurves/blob/main/UserManualResources/step4.png?raw=true" alt="Length Step 4" style="width: 350px;"/> </td>
</tr>
<tr>
  <td>Length Step Size is 0.5</td>
  <td>Length Step Size is 1</td>
  <td>Length Step Size is 4</td>
</tr></table>

__Round Tile to Nearest:__  
This controls how the faces that make up the curve and the uvs are rounded to better fit the curve. For example if there is a curve of length 21.4 when __Round Tile to Nearest__ is set to 0 no rounding will occur so the curve will have 21 tiles plus a last tile of length 0.4 the rest of the tiles. If __Round Tile to Nearest__ is set to 1 then the curve will be rounded to length 21 so it will have 21 tiles. If __Round Tile to Nearest__ is set to 4 then the curve will be rounded to length 20 and have 20 tiles.

<table><tr>
  <td> <img src="https://github.com/J2-2/MarbleCurves/blob/main/UserManualResources/round0.png?raw=true" alt="Round 0" style="width: 350px;"/> </td>
  <td> <img src="https://github.com/J2-2/MarbleCurves/blob/main/UserManualResources/round1.png?raw=true" alt="Round 1" style="width: 350px;"/> </td>
  <td> <img src="https://github.com/J2-2/MarbleCurves/blob/main/UserManualResources/round4.png?raw=true" alt="Round 4" style="width: 350px;"/> </td>
</tr>
<tr>
  <td>Round Tile to Nearest is 0</td>
  <td>Round Tile to Nearest is 1</td>
  <td>Round Tile to Nearest is 4</td>
</tr></table>

<a name="tube_mode"></a>
### Tube Mode

Ractangle mode can be selected by choosing the __tube__ option in the mode selector. This mode makes it so that the cross section of the curve will be a circle with an inner and outer edge specified by __Inner Radius__ and __Outer Randius__. The middle of the curve will be centered on the middle of the circle.

__<ins>Parameters</ins>__

__Inner Radius/Outer Radius:__  
These determine the size and thickness of the circle cross section of the curve. __Inner Radius__ is the distance from the center to the inner edge and __Outer Radius__ is the distance to the outside edge. So the thickness of the circle will be __Outer Radius__ - __Inner Radius__.

<table><tr>
  <td> <img src="https://github.com/J2-2/MarbleCurves/blob/main/UserManualResources/inner4outer5.png?raw=true" alt="Inner 4, Outer 5" style="width: 350px;"/> </td>
  <td> <img src="https://github.com/J2-2/MarbleCurves/blob/main/UserManualResources/inner1outer3.png?raw=true" alt="Inner 1, Outer 3" style="width: 350px;"/> </td>
  <td> <img src="https://github.com/J2-2/MarbleCurves/blob/main/UserManualResources/inner2outer2.5.png?raw=true" alt="Inner 2, Outer 2.5" style="width: 350px;"/> </td>
</tr>
<tr>
  <td>Inner Radius is 4 and Outer Radius is 5</td>
  <td>Inner Radius is 1 and Outer Radius is 3</td>
  <td>Inner Radius is 2 and Outer Radius is 2.5</td>
</tr></table>

__Width Offset/Height Offset__:  
These control how offset the cross section is from the center of the curve. When set to 0 and 0 the center of the curve is positioned in the middle of the circle cross section.

<table><tr>
  <td> <img src="https://github.com/J2-2/MarbleCurves/blob/main/UserManualResources/offset_t2_0.png?raw=true" alt="Width Offset 2, Height Offset 0" style="width: 350px;"/> </td>
  <td> <img src="https://github.com/J2-2/MarbleCurves/blob/main/UserManualResources/offset_t-4_4.png?raw=true" alt="Width Offset -4, Height Offset 4" style="width: 350px;"/> </td>
  <td> <img src="https://github.com/J2-2/MarbleCurves/blob/main/UserManualResources/offset_t0_3.png?raw=true" alt="Width Offset 0, Height Offset 3" style="width: 350px;"/> </td>
</tr>
<tr>
  <td>Width Offset is 2 and Height Offset is 0</td>
  <td>Width Offset is -4 and Height Offset is 4</td>
  <td>Width Offset is 0 and Height Offset is 3</td>
</tr></table>

__Length Step Size:__  
This controls the length the faces that the curve will be subdivided into. The smaller the size of the length step the smoother the geometry will be but the curve will take up more memory and take longer to make. Using larger face sizes can create a jagged edge on the curve.

<table><tr>
  <td> <img src="https://github.com/J2-2/MarbleCurves/blob/main/UserManualResources/step_t0.5.png?raw=true" alt="Length Step 0.5" style="width: 350px;"/> </td>
  <td> <img src="https://github.com/J2-2/MarbleCurves/blob/main/UserManualResources/step_t1.png?raw=true" alt="Length Step 1" style="width: 350px;"/> </td>
  <td> <img src="https://github.com/J2-2/MarbleCurves/blob/main/UserManualResources/step_t4.png?raw=true" alt="Length Step 4" style="width: 350px;"/> </td>
</tr>
<tr>
  <td>Length Step Size is 0.5</td>
  <td>Length Step Size is 1</td>
  <td>Length Step Size is 4</td>
</tr></table>

__Divisions:__  
This controls the numbers of faces the circumference of the circle will be made up of. More divisions wil make the curve smoother but take up more memory and take loger to make.

<table><tr>
  <td> <img src="https://github.com/J2-2/MarbleCurves/blob/main/UserManualResources/divisions32.png?raw=true" alt="Divisions 32" style="width: 350px;"/> </td>
  <td> <img src="https://github.com/J2-2/MarbleCurves/blob/main/UserManualResources/divisions16.png?raw=true" alt="Divisions 16" style="width: 350px;"/> </td>
  <td> <img src="https://github.com/J2-2/MarbleCurves/blob/main/UserManualResources/divisions8.png?raw=true" alt="Divisions 8" style="width: 350px;"/> </td>
</tr>
<tr>
  <td>32 Divisions</td>
  <td>16 Divisions</td>
  <td>8 Divisions</td>
</tr></table>


__Round Tile to Nearest:__  
This controls how the faces that make up the curve and the uvs are rounded to better fit the curve. For example if there is a curve of length 21.4 when __Round Tile to Nearest__ is set to 0 no rounding will occur so the curve will have 21 tiles plus a last tile of length 0.4 the rest of the tiles. If __Round Tile to Nearest__ is set to 1 then the curve will be rounded to length 21 so it will have 21 tiles. If __Round Tile to Nearest__ is set to 4 then the curve will be rounded to length 20 and have 20 tiles.

<table><tr>
  <td> <img src="https://github.com/J2-2/MarbleCurves/blob/main/UserManualResources/round_t0.png?raw=true" alt="Round 0" style="width: 350px;"/> </td>
  <td> <img src="https://github.com/J2-2/MarbleCurves/blob/main/UserManualResources/round_t1.png?raw=true" alt="Round 1" style="width: 350px;"/> </td>
  <td> <img src="https://github.com/J2-2/MarbleCurves/blob/main/UserManualResources/round_t4.png?raw=true" alt="Round 4" style="width: 350px;"/> </td>
</tr>
<tr>
  <td>Round Tile to Nearest is 0</td>
  <td>Round Tile to Nearest is 1</td>
  <td>Round Tile to Nearest is 4</td>
</tr></table>

<a name="custom_mode"></a>
### Custom Mode

Custom mode can be selected by choosing the __custom__ option in the mode selector. This mode makes it so that the cross section of the curve will be the shape of the chosen __Cross Section__. The middle of the curve will be centered on the pivot of the __Cross Section__ object.

__<ins>Parameters</ins>__

__Cross Section:__  
This is a GameObject that determines the shape of the cross section of the curve. A GameObject can be chosen by dragging it into the __Cross Section__ box in the Marble Curves window or by selecting it and pressing the __Use Selected Button__. The selected GameObject must have a 3D mesh to be used. The edges of the faces of object along x = 0 relative to the pivot will be used so make sure that the pivot is in the right location of the object selected. Faces entirely within x = 0 will be used at the start and the end of the curve. UV information from the GameObject will be used, If the UVs on one side are messed up (the face will apear black with the default texture) then make sure the UVs on that face of the the __Cross Section__ GameObject are not rotated. Some examples of diefferent __Cross Section__ gameobjects with pivots visualized and the curves they generate are below.

<table><tr>
  <td> <img src="https://github.com/J2-2/MarbleCurves/blob/main/UserManualResources/custom1.png?raw=true" alt="Custom Cross Section #1" style="width: 350px;"/> </td>
  <td> <img src="https://github.com/J2-2/MarbleCurves/blob/main/UserManualResources/custom2_.png?raw=true" alt="Custom Cross Section #2" style="width: 350px;"/> </td>
  <td> <img src="https://github.com/J2-2/MarbleCurves/blob/main/UserManualResources/custom3.png?raw=true" alt="Custom Cross Section #3" style="width: 350px;"/> </td>
</tr></table>

__Width Offset/Height Offset__:  
These control how offset the cross section is from the center of the curve. When set to 0 and 0 the center of the curve is positioned on the pivot of the selected __Cross Section__ GameObject.

<table><tr>
  <td> <img src="https://github.com/J2-2/MarbleCurves/blob/main/UserManualResources/offset_c6_0_.png?raw=true" alt="Width Offset 6, Height Offset 0" style="width: 350px;"/> </td>
  <td> <img src="https://github.com/J2-2/MarbleCurves/blob/main/UserManualResources/offset_c-4_4_.png?raw=true" alt="Width Offset -4, Height Offset 4" style="width: 350px;"/> </td>
  <td> <img src="https://github.com/J2-2/MarbleCurves/blob/main/UserManualResources/offset_c0_-1_.png?raw=true" alt="Width Offset 0, Height Offset -1" style="width: 350px;"/> </td>
</tr>
<tr>
  <td>Width Offset is 6 and Height Offset is 0</td>
  <td>Width Offset is -4 and Height Offset is 4</td>
  <td>Width Offset is 0 and Height Offset is -1</td>
</tr></table>

__Length Step Size:__  
This controls the length the faces that the curve will be subdivided into. The smaller the size of the length step the smoother the geometry will be but the curve will take up more memory and take longer to make. Using larger face sizes can create a jagged edge on the curve.

<table><tr>
  <td> <img src="https://github.com/J2-2/MarbleCurves/blob/main/UserManualResources/step_c0.5.png?raw=true" alt="Length Step 0.5" style="width: 350px;"/> </td>
  <td> <img src="https://github.com/J2-2/MarbleCurves/blob/main/UserManualResources/step_c1.png?raw=true" alt="Length Step 1" style="width: 350px;"/> </td>
  <td> <img src="https://github.com/J2-2/MarbleCurves/blob/main/UserManualResources/step_c4.png?raw=true" alt="Length Step 4" style="width: 350px;"/> </td>
</tr>
<tr>
  <td>Length Step Size is 0.5</td>
  <td>Length Step Size is 1</td>
  <td>Length Step Size is 4</td>
</tr></table>

### Shape
__rectange__: The cross section of the curve will be a rectangle.    
__tube__: The cross section of the curve will be a circle.    
__custom__: The cross section of the curve will be based on the object set in __Cross Section__. The cross section will be made from the edges at x position 0 relative to the pivot of the object so make sure the pivot is in the right place.    
__fit to curve__: The curve will be made by bending the object set in __Object to Fit__ to the curve. __Offset__ is the offset to where the object starts being fit  to the curve. __Stretch to Fit Curve__ will make the end of the object line up with the end of the curve. If the object is longer than the curve make sure to select __Stretch to Fit Curve__ so it will fit.

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

## Credits

Made By J2_2

Special Thanks to Hyran for his help testing
