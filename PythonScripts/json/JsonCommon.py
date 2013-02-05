def loadStuff():
    import sys
    import os
    sys.path.append(os.path.dirname(__file__))
    sys.path.append(os.path.join(os.path.dirname(__file__), "bin"))
    import clr
    clr.AddReference("Newtonsoft.Json.dll")
    
loadStuff()
del loadStuff
from Newtonsoft.Json.Schema import *
from Newtonsoft.Json import *