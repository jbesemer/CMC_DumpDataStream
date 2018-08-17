This Solution features a self-contained example desktop VS C#/.Net WPF app,
called DumpDataStream.

DumpDataStream relies on another solution, called SharedLibrary. This 
SharedLibrary solution should be installed in the same folder as this one. 
That way the reference and project linkages from here to there will work. 
You can put it someplace else but then you'll have to redo the linkages 
from this solution to the other, and there's no point.

DumpDataStream originated as a component within the overall CMC project, and
relies on a component there, called CMC_Library. However, CMC_Library is 
huge and is largely irrelevant to this example. Rather than creating a 
dependency to the CMC solution itself, I instead copied the several files 
needed into a local library called CMC_Library within this solution.

Finally the solution contains a few example data files for running and
testing the app. When you start, the app defaults to the Desktop folder,
so you'll have to navigate to this solution folder to open the file. 
But after the first invociation the app will default to that same folder.

This was a quick and dirty app for my own debugging, so there surely are
numerous obvious possible improvements to the UI. 

Folder/SVN names: CMC_DumpDataStream, SharedLibrary
Solution names: DumpDataStream, SharedLibrary
Project names: DumpDataStream, SharedLibrary, CMC_Library
