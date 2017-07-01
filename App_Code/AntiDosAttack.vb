
Namespace DebModules
    Public Class AntiDosAttack
        Shared ReadOnly items As New List(Of IpObject)()
        Shared SleepTime As Integer = 0
        Shared MaxAllowedCount As Integer
        Shared AllowedSeconds_ As Integer
        Public Shared Sub Monitor(Capacity As Integer, Seconds2Sleep As Integer, AllowedCount As Integer, Optional AllowedSeconds As Integer = 120)
            SleepTime = Seconds2Sleep
            MaxAllowedCount = AllowedCount
            AllowedSeconds_ = AllowedSeconds
            Dim ip As String = HttpContext.Current.Request.UserHostAddress

            If ip = "" Then
                Return
            End If

            ' This part to exclude some known requesters
            If HttpContext.Current.Request.UserAgent IsNot Nothing AndAlso HttpContext.Current.Request.UserAgent = "Some good bots" Then
                Return
            End If

            'remove old expired requests from collection
            items.RemoveAll(AddressOf ItemExpiresTheSleepTime)

            Dim lastTrackedIP = (From t In items Where t.IP = ip Select t).FirstOrDefault
            If lastTrackedIP IsNot Nothing Then
                ' increase the request count per second
                lastTrackedIP.Count += 1
                If lastTrackedIP.OverRequestTime = Nothing And lastTrackedIP.Count > AllowedCount Then
                    lastTrackedIP.OverRequestTime = DateTime.Now
                End If


            Else
                'Add new request
                lastTrackedIP = New IpObject(ip)
                items.Insert(0, lastTrackedIP)
            End If


            ' Trim collection capacity is full. remove from bottom of the queue
            If items.Count > Capacity Then
                items.RemoveAt(items.Count - 1)
            End If

            ' Count of current IP in collection
            Dim count As Integer = lastTrackedIP.Count

            ' Decide on block or bypass
            If lastTrackedIP.Count > AllowedCount And lastTrackedIP.OverRequestTime <> Nothing And (lastTrackedIP.OverRequestTime - lastTrackedIP.Date).TotalSeconds <= AllowedSeconds Then
                'Add the IP to block list or send alert email.
                'ErrorReport.Report.ToWebmaster(New Exception("Blocked probable ongoing ddos attack"), "EvrinHost 24 / 7 Support - DDOS Block", "")

                ' create a response code 429 or whatever needed and end response
                HttpContext.Current.Response.StatusCode = 429
                HttpContext.Current.Response.StatusDescription = "Too Many Requests, Slow down Cowboy!"
                HttpContext.Current.Response.Write("Too Many Requests")
                HttpContext.Current.Response.Flush()
                ' Sends all currently buffered output to the client.
                HttpContext.Current.Response.SuppressContent = True
                ' Gets or sets a value indicating whether to send HTTP content to the client.
                ' Causes ASP.NET to bypass all events and filtering in the HTTP pipeline chain of execution and directly execute the EndRequest event.
                HttpContext.Current.ApplicationInstance.CompleteRequest()

            End If

        End Sub

        Private Shared Function ItemExpiresTheSleepTime(obj As IpObject) As Boolean
            If obj.OverRequestTime <> Nothing Then
                Return (DateTime.Now - obj.OverRequestTime).TotalSeconds > SleepTime
            End If
            Return (DateTime.Now - obj.[Date]).TotalSeconds > SleepTime
        End Function


        Friend Class IpObject
            Public Sub New(ip__1 As String)
                IP = ip__1
                [Date] = DateTime.Now
                Count = 1
            End Sub

            Public Property IP() As String
                Get
                    Return m_IP
                End Get
                Set
                    m_IP = Value
                End Set
            End Property
            Private m_IP As String
            Public Property [Date]() As DateTime
                Get
                    Return m_Date
                End Get
                Set
                    m_Date = Value
                End Set
            End Property
            Private m_Date As DateTime

            Private count_ As Int16
            Public Property Count() As Int16
                Get
                    Return count_
                End Get
                Set(ByVal value As Int16)
                    count_ = value
                End Set
            End Property
            Private overRequestTime_ As DateTime = Nothing
            Public Property OverRequestTime() As DateTime
                Get
                    Return overRequestTime_
                End Get
                Set(ByVal value As DateTime)
                    overRequestTime_ = value
                End Set
            End Property
        End Class
    End Class
End Namespace



