def SplitXlsxToCsv(path):
    from System.Type import Missing
    from System.IO import File
    from System.IO import Directory
    import clr
    import System
    clr.AddReference(r'Microsoft.Office.Interop.Excel')
    from Microsoft.Office.Interop import Excel
    app = Excel.ApplicationClass()
    app.Visible = True
    for filePath in Directory.GetFiles(path, '*.xlsx', System.IO.SearchOption.AllDirectories):
        book = app.Workbooks.Open(filePath, 0, False, 5, "", "", False, Excel.XlPlatform.xlWindows, "", True, False, 0, True, False, False)
        for sheetIndex in range(book.Sheets.Count):
            try:
                book.sheets[sheetIndex + 1].Select(Missing)
                book.SaveAs(filePath.replace(".xlsx","." + book.sheets[sheetIndex + 1].Name + ".csv"), Excel.XlFileFormat.xlCSVMSDOS, "", False, False, False, Excel.XlSaveAsAccessMode.xlNoChange, Excel.XlSaveConflictResolution.xlLocalSessionChanges, False, Missing, Missing, Missing)
            except:
                print "Error selecting sheet number " + str(sheetIndex + 1) + " from '" + filePath + "'"
        book.Close(False, "", False)
    app.Quit()

if __name__ == "__main__":
    import sys
    SplitXlsxToCsv(sys.argv[1])