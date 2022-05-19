using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CSVLogger
{
    private string filebuffer;

    public void AddToBuffer(string _inp)
    {
        filebuffer += _inp;
    }

    public void ExportBufferToCSV(string _filename)
    {
        File.WriteAllText(_filename, filebuffer);
    }

    public void ClearBuffer()
    {
        
    }
}
