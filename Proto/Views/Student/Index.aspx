﻿<%@ Register Assembly="CKEditor.NET" Namespace="CKEditor.NET" TagPrefix="CKEditor" %>

<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<!DOCTYPE html>

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
        <asp:Label runat="server">
            <iframe width="500" height="300" src="//www.youtube.com/embed/pQcnLGBi7WE" frameborder="0" allowfullscreen></iframe>
        </asp:Label>
        <br><br>
        <textarea cols="80" id="editor1" name="editor1" rows="10" >
	</textarea>
	<script>
	    CKEDITOR.replace('editor1', {
	        width: '60%',
	        height: 100,
	        contentsCss: "body{background: url(http://encs.vancouver.wsu.edu/~t.roper/paper.jpg) no-repeat; background-size: cover;}"
	    });
	    
	</script>
    <div>
        
    </div>
</body>
</html>