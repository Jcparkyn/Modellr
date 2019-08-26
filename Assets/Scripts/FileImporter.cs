using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FileImporter : MonoBehaviour
{
    string startupFunction => @"
            document.addEventListener('click', function() {

                var fileuploader = document.getElementById('fileuploader');
                if (!fileuploader) {
                    fileuploader = document.createElement('input');
                    fileuploader.setAttribute('style','display:none;');
                    fileuploader.setAttribute('type', 'file');
                    fileuploader.setAttribute('id', 'fileuploader');
                    fileuploader.setAttribute('class', 'focused');
                    document.getElementsByTagName('body')[0].appendChild(fileuploader);

                    fileuploader.onchange = function(e) {
                    var files = e.target.files;
                        for (var i = 0, f; f = files[i]; i++) {
                            //window.alert(URL.createObjectURL(f));
                            SendMessage('Mesh', 'FileDialogResult', URL.createObjectURL(f));
                        }
                    };
                }
                if (fileuploader.getAttribute('class') == 'focused') {
                    fileuploader.setAttribute('class', '');
                    fileuploader.click();
                }
            });";

    string importFileFunction => @"
            var fileuploader = document.getElementById('fileuploader');
            if (fileuploader) {
                fileuploader.setAttribute('class', 'focused');
            }";

    public void Start()
    {
        //Application.ExternalEval(startupFunction);
    }

    public void ImportFile()
    {
        //Application.ExternalEval(importFileFunction);
    }

    
}
