from JsonCommon import *
import System
import os

def GenerateSchemaFromType(assemblyPath, typeName):
    import clr
    clr.AddReferenceToFileAndPath(assemblyPath)
    asses = System.AppDomain.CurrentDomain.GetAssemblies()
    type = None
    for ass in asses:
        for t in ass.GetTypes():
            if t.FullName.lower() == typeName.lower():
                type = t
                break
    if type == None:
        print "//Error: could not resolve type, '{0}'".format(typeName)
        return None
    gen = JsonSchemaGenerator()
    schema = gen.Generate(type)
    return schema
    
def PrintSchemaToStdOut(schema):
    import sys
    txtWriter = System.IO.StringWriter()
    jsonTxtWriter = JsonTextWriter(txtWriter)
    jsonTxtWriter.Formatting  = Formatting.Indented
    schema.WriteTo(jsonTxtWriter)
    print str(txtWriter)
    txtWriter.Dispose()
    print ""
    
if __name__ == "__main__":
    import sys
    if len(sys.argv) <= 1:
        print "ipy GenSchemaFromType.py PATH_TO_ASSEMBLY_TO_REFERENCE FIRST_TYPE_AND_NAMESPACE SECOND_TYPE_AND_NAMESPACE ..."
    else:
        assemblyPath = os.path.abspath(sys.argv[1])
        for i in xrange(2, len(sys.argv)):
            print "//Schema for type '{0}'".format(sys.argv[i])
            s = GenerateSchemaFromType(assemblyPath, sys.argv[i])
            if s != None:
                PrintSchemaToStdOut(s)