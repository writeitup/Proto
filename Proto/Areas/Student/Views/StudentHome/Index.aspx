﻿<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>
<%@ Register Assembly="CKEditor.NET" Namespace="CKEditor.NET" TagPrefix="CKEditor" %>

<!DOCTYPE HTML>

    <html>
    <head runat="server">
        <meta charset="utf-8">
        <meta name="viewports" content="noindex, nofollow">
        <title>Student Main Page</title>
        <script src="//cdn.ckeditor.com/4.4.5/full/ckeditor.js"></script>
    </head>
    <body style="
        background-image: url(http://encs.vancouver.wsu.edu/~k.gonzalez/letters3.50.jpg);
        background-repeat: no-repeat;
        background-size: cover">

        Educational Video Regarding the Writing Assignment
        <br>
        <asp:label runat="server">
            <iframe width="500" height="300" src="//www.youtube.com/embed/pQcnLGBi7WE" frameborder="0" allowfullscreen></iframe><br>
            <iframe width="900" height="625" src="http://mudcu.be/sketchpad/" frameborder="0"></iframe>
        </asp:label>
        <br><br>
        <form>
            <textarea cols="80" id="editor1" name="editor1" rows="10"></textarea>
            <script>
                CKEDITOR.replace('editor1', {
                    width: '81%',
                    height: 300,
                    contentsCss: "body{background: url(http://encs.vancouver.wsu.edu/~t.roper/paper.jpg) no-repeat; background-size: cover;}",
                    toolbar: [
                        { name: 'document', groups: ['mode', 'document', 'doctools'], items: ['Save', 'NewPage'] },
                        { name: 'clipboard', groups: ['clipboard', 'undo'], items: ['Cut', 'Copy', 'Paste', '-', 'Undo', 'Redo'] },
                        { name: 'editing', groups: ['find', 'selection'], items: ['Find', '-', 'SelectAll', '-'] },
                        { name: 'insert', items: ['Image', 'Table', 'HorizontalRule', 'SpecialChar', 'PageBreak'] },
                        { name: 'paragraph', groups: ['list', 'indent', 'blocks', 'align', 'bidi'], items: ['NumberedList', 'BulletedList', '-', 'Outdent', 'Indent', '-', 'Blockquote', '-', 'JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyBlock', '-', 'BidiLtr', 'BidiRtl'] },
                        '/',
                        { name: 'basicstyles', groups: ['basicstyles', 'cleanup'], items: ['Bold', 'Italic', 'Underline', 'Strike', 'Subscript', 'Superscript', '-', 'RemoveFormat'] },
                        { name: 'styles', items: ['Styles', 'Format', 'Font', 'FontSize'] },
                        { name: 'colors', items: ['TextColor', 'BGColor'] },
                        { name: 'tools', items: ['Maximize', 'ShowBlocks'] },
                        { name: 'others', items: ['-'] },
                    ]
                });
                var editor = CKEDITOR.replace('editor1');
                Debug.WriteLine("Data");
                // The "save" event is fired whenever a change is made in the editor.
                editor.on('save', function (evt) {
                    // getData() returns CKEditor's HTML content.
                    //console.log('data: ' + evt.editor.getData());
                    var data = evt.editor.getData();
                    Debug.WriteLine("Data");
                    // var data = evt.editor.getData();
                    // Save data
                });

                // The "change" event is fired whenever a change is made in the editor.
                editor.on('change', function (evt) {
                    // getData() returns CKEditor's HTML content.
                    //console.log('data: ' + evt.editor.getData());
                    var data = evt.editor.getData();
                    Debug.WriteLine("Data");
                    // Save data
                });
            </script>
        </form>
        <div>

        </div>
    </body>
</html>
