


Imports System.IO
Imports System.Net
Imports System.Net.NetworkInformation
Imports NATUPNPLib
Imports UPNPLib


<ComVisible(False)>
Public Class SetupDialogForm

    Private Sub OK_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK_Button.Click ' OK button event handler
        ' Persist new values of user settings to the ASCOM profile
        Camera.IPAddress = CBCameraIPAddress.SelectedItem.ToString ' Update the state variables with results from the dialogue
        Camera.traceState = chkTrace.Checked
        Me.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.Close()
    End Sub

    ''' <summary>
    ''' GetIpNetTable external method
    ''' </summary>
    ''' <param name="pIpNetTable"></param>
    ''' <param name="pdwSize"></param>
    ''' <param name="bOrder"></param>
    ''' <returns></returns>
    <DllImport("IpHlpApi.dll")>
    Private Shared Function GetIpNetTable(pIpNetTable As IntPtr, <MarshalAs(UnmanagedType.U4)> ByRef pdwSize As Integer, bOrder As Boolean) As <MarshalAs(UnmanagedType.U4)> Integer
    End Function

    ''' <summary>
    ''' Error codes GetIpNetTable returns that we recognise
    ''' </summary>
    Const ERROR_INSUFFICIENT_BUFFER As Integer = 122
    ''' <summary>
    ''' MIB_IPNETROW structure returned by GetIpNetTable
    ''' DO NOT MODIFY THIS STRUCTURE.
    ''' </summary>
    <StructLayout(LayoutKind.Sequential)>
    Private Structure MIB_IPNETROW
        <MarshalAs(UnmanagedType.U4)>
        Public dwIndex As Integer
        <MarshalAs(UnmanagedType.U4)>
        Public dwPhysAddrLen As Integer
        <MarshalAs(UnmanagedType.U1)>
        Public mac0 As Byte
        <MarshalAs(UnmanagedType.U1)>
        Public mac1 As Byte
        <MarshalAs(UnmanagedType.U1)>
        Public mac2 As Byte
        <MarshalAs(UnmanagedType.U1)>
        Public mac3 As Byte
        <MarshalAs(UnmanagedType.U1)>
        Public mac4 As Byte
        <MarshalAs(UnmanagedType.U1)>
        Public mac5 As Byte
        <MarshalAs(UnmanagedType.U1)>
        Public mac6 As Byte
        <MarshalAs(UnmanagedType.U1)>
        Public mac7 As Byte
        <MarshalAs(UnmanagedType.U4)>
        Public dwAddr As Integer
        <MarshalAs(UnmanagedType.U4)>
        Public dwType As Integer
    End Structure

    ''' <summary>
    ''' Get the IP and MAC addresses of all known devices on the LAN
    ''' </summary>
    ''' <remarks>
    ''' 1) This table is not updated often - it can take some human-scale time
    '''    to notice that a device has dropped off the network, or a new device
    '''    has connected.
    ''' 2) This discards non-local devices if they are found - these are multicast
    '''    and can be discarded by IP address range.
    ''' </remarks>
    ''' <returns></returns>
    Public Shared Function GetAllDevicesOnLAN() As Dictionary(Of IPAddress, PhysicalAddress)
        Dim all As New Dictionary(Of IPAddress, PhysicalAddress)()
        ' Add this PC to the list...
        'all.Add(GetIPAddress(), GetMacAddress())
        Dim spaceForNetTable As Integer = 0
        ' Get the space needed
        ' We do that by requesting the table, but not giving any space at all.
        ' The return value will tell us how much we actually need.
        GetIpNetTable(IntPtr.Zero, spaceForNetTable, False)
        ' Allocate the space
        ' We use a try-finally block to ensure release.
        Dim rawTable As IntPtr = IntPtr.Zero
        Try
            rawTable = Marshal.AllocCoTaskMem(spaceForNetTable)
            ' Get the actual data
            Dim errorCode As Integer = GetIpNetTable(rawTable, spaceForNetTable, False)
            If errorCode <> 0 Then
                ' Failed for some reason - can do no more here.
                Throw New Exception(String.Format("Unable to retrieve network table. Error code {0}", errorCode))
            End If
            ' Get the rows count
            Dim rowsCount As Integer = Marshal.ReadInt32(rawTable)
            Dim currentBuffer As New IntPtr(rawTable.ToInt64() + Marshal.SizeOf(GetType(Int32)))
            ' Convert the raw table to individual entries
            Dim rows As MIB_IPNETROW() = New MIB_IPNETROW(rowsCount - 1) {}
            For index As Integer = 0 To rowsCount - 1
                rows(index) = CType(Marshal.PtrToStructure(New IntPtr(currentBuffer.ToInt64() + (index * Marshal.SizeOf(GetType(MIB_IPNETROW)))), GetType(MIB_IPNETROW)), MIB_IPNETROW)
            Next
            ' Define the dummy entries list (we can discard these)
            Dim virtualMAC As New PhysicalAddress(New Byte() {0, 0, 0, 0, 0, 0})
            Dim broadcastMAC As New PhysicalAddress(New Byte() {255, 255, 255, 255, 255, 255})
            For Each row As MIB_IPNETROW In rows
                Dim ip As New IPAddress(BitConverter.GetBytes(row.dwAddr))
                Dim rawMAC As Byte() = New Byte() {row.mac0, row.mac1, row.mac2, row.mac3, row.mac4, row.mac5}
                Dim pa As New PhysicalAddress(rawMAC)
                If Not pa.Equals(virtualMAC) AndAlso Not pa.Equals(broadcastMAC) Then
                    'Console.WriteLine("IP: {0}\t\tMAC: {1}", ip.ToString(), pa.ToString());
                    If Not all.ContainsKey(ip) Then
                        all.Add(ip, pa)
                    End If
                End If
            Next
        Finally
            ' Release the memory.
            Marshal.FreeCoTaskMem(rawTable)
        End Try
        Return all
    End Function


    Private Sub Cancel_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel_Button.Click 'Cancel button event handler
        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Close()
    End Sub

    Private Sub ShowAscomWebPage(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PictureBox1.DoubleClick, PictureBox1.Click
        ' Click on ASCOM logo event handler
        Try
            System.Diagnostics.Process.Start("http://ascom-standards.org/")
        Catch noBrowser As System.ComponentModel.Win32Exception
            If noBrowser.ErrorCode = -2147467259 Then
                MessageBox.Show(noBrowser.Message)
            End If
        Catch other As System.Exception
            MessageBox.Show(other.Message)
        End Try
    End Sub

    Private Sub SetupDialogForm_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load ' Form load event handler
        ' Retrieve current values of user settings from the ASCOM Profile
        InitUI()

    End Sub

    Public Function FindMediaService() As Boolean
        Dim MediaServer As UPNPLib.UPnPDevice
        Dim ActionArgs As New Object ' must be instantiated
        Dim deviceFinder = CreateObject("UPnP.UPnPDeviceFinder")
        Dim devices = deviceFinder.FindByType("urn:schemas-upnp-org:service:ContentDirectory:1", 0)

        Dim FoundService As Boolean = False
        For Each MediaServer In devices
            If MediaServer.ModelName = "Lumix" Then
                Camera.LumixService = MediaServer
                FoundService = True
                Return FoundService
            End If
        Next
        Return FoundService
    End Function


    Private Sub InitUI()
        Dim request As WebRequest
        Dim myStreamReader As StreamReader
        Dim SendStatus As Integer = -1
        Dim statusCode As HttpStatusCode
        Dim ResponseText As String


        chkTrace.Checked = Camera.traceState

        Dim IPValues As New List(Of IPAddress)(GetAllDevicesOnLAN().Keys)
        CBCameraIPAddress.Items.Clear()
        CBCameraIPAddress.DataSource = New BindingSource(IPValues, Nothing)
        ' select the current IPAddress if possible
        If CBCameraIPAddress.Items.Contains(Camera.IPAddress) Then
            CBCameraIPAddress.SelectedItem = Camera.IPAddress
        End If
        'trying to connect to the Lumix Cam

        For Each TryIPValue As IPAddress In IPValues
            'request = WebRequest.Create("http://" + TryIPValue.ToString + "/cam.cgi?mode=accctrl&type=req_acc&value=4D454930-0100-1000-8001-024500021C98&value2=SM-G903F HTTP/1.1")
            request = WebRequest.Create("http://" + TryIPValue.ToString + "/" + Camera.STATE)
            'Camera.SendLumixMessage(Camera.ISO + CBISO.SelectedItem.ToString)
            Try
                Dim myWebResponse = CType(request.GetResponse(), HttpWebResponse)
                myStreamReader = New StreamReader(myWebResponse.GetResponseStream())
                ResponseText = myStreamReader.ReadToEnd
                If myWebResponse.StatusCode = HttpStatusCode.Accepted Or myWebResponse.StatusCode = 200 Then
                    SendStatus = 1 'message sent successfully
                    CBCameraIPAddress.SelectedItem = TryIPValue
                    Camera.IPAddress = TryIPValue.ToString
                    Exit For
                Else
                    SendStatus = 2 'message processed but not sent successfully
                End If
            Catch e As WebException
                If (e.Status = WebExceptionStatus.ProtocolError) Then
                    Dim response As WebResponse = e.Response
                    Using (response)
                        Dim httpResponse As HttpWebResponse = CType(response, HttpWebResponse)
                        statusCode = httpResponse.StatusCode
                        Try
                            myStreamReader = New StreamReader(response.GetResponseStream())
                            Using (myStreamReader)
                                ResponseText = myStreamReader.ReadToEnd & "Status Description = " & httpResponse.StatusDescription ' HttpWebResponse.StatusDescription
                            End Using
                        Catch ex As Exception
                            'TL.LogMessage("Message Sent Failed", "Message Sent  failed")
                        End Try
                    End Using
                End If
            End Try
        Next
    End Sub


    Private Sub CameraIPAddress_SelectedIndexChanged(sender As Object, e As EventArgs) Handles CBCameraIPAddress.SelectedIndexChanged
        Camera.IPAddress = CBCameraIPAddress.SelectedItem.ToString
    End Sub

    Private Sub ButtonApplySettings_Click(sender As Object, e As EventArgs) Handles ButtonApplySettings.Click
        If Camera.IPAddress IsNot Camera.IPAddressDefault Then
            If CBISO.SelectedItem Then
                Camera.SendLumixMessage(Camera.ISO + CBISO.SelectedItem.ToString)
            Else
                Camera.SendLumixMessage(Camera.ISO + "800")
            End If
            If CBSpeed.SelectedIndex Then
                Camera.SendLumixMessage(Camera.SHUTTERSPEED + Camera.ShutterTable(CBSpeed.SelectedIndex - 1, 0))
            Else
                Camera.SendLumixMessage(Camera.SHUTTERSPEED + Camera.ShutterTable(55, 0))
            End If
            ' switching to play mode
            Camera.SendLumixMessage("/cam.cgi?mode=camcmd&value=playmode")
            FindMediaService()
            End If

    End Sub


End Class
