<%@ Application Language="VB" %>
<%@ Import Namespace="System.Web.Optimization" %>
<%@ Import Namespace="System.Web.Routing" %>

<script runat="server">

    Sub Application_Start(sender As Object, e As EventArgs)
        RouteConfig.RegisterRoutes(RouteTable.Routes)
        BundleConfig.RegisterBundles(BundleTable.Bundles)
    End Sub

    Protected Sub Application_BeginRequest(sender As [Object], e As EventArgs)
        Dim app As HttpApplication = CType(sender, HttpApplication)
        Dim filePath As String = app.Context.Request.FilePath
        Dim fileExtension As String = VirtualPathUtility.GetExtension(filePath)
        If fileExtension.Equals(".aspx") Then
            DebModules.AntiDosAttack.Monitor(1000, 1800, 30, 30)
        End If
    End Sub
</script>