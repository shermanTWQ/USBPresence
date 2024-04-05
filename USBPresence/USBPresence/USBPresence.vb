Imports System.Management ' Also had to add System.Management as a reference
Public Class USBPresence

    ' USB Presence 1.0
    'Provided for non-commercial use. Please refer to Github Repo for more information. 
    'Contact: Sherman_tan@outlook.com
    '(C) Tan Wei Qiang Sherman. 


    Private WithEvents m_MediaConnectWatcher As ManagementEventWatcher
    Public USBDriveName As String
    Public USBDriveLetter As String

    Public Sub StartDetection()
        ' __InstanceOperationEvent will trap both Creation and Deletion of class instances
        Dim query2 As New WqlEventQuery("SELECT * FROM __InstanceOperationEvent WITHIN 1 " _
  & "WHERE TargetInstance ISA 'Win32_DiskDrive'")

        m_MediaConnectWatcher = New ManagementEventWatcher
        m_MediaConnectWatcher.Query = query2
        m_MediaConnectWatcher.Start()
    End Sub


    Private Sub Arrived(ByVal sender As Object, ByVal e As System.Management.EventArrivedEventArgs) Handles m_MediaConnectWatcher.EventArrived

        Dim mbo, obj As ManagementBaseObject
        ' the first thing we have to do is figure out if this is a creation or deletion event
        mbo = CType(e.NewEvent, ManagementBaseObject)
        ' next we need a copy of the instance that was either created or deleted
        obj = CType(mbo("TargetInstance"), ManagementBaseObject)

        Select Case mbo.ClassPath.ClassName
            Case "__InstanceCreationEvent"
                If obj("InterfaceType") = "USB" Then
                    MsgBox(obj("Caption") & " (Drive letter " & GetDriveLetterFromDisk(obj("Name")) & ") has been plugged into the system.")

                Else
                    MsgBox(obj("InterfaceType"))
                End If
            Case "__InstanceDeletionEvent"
                If obj("InterfaceType") = "USB" Then
                    MsgBox(obj("Caption") & " has been unplugged from the system.")
                    If obj("Caption") = USBDriveName Then
                        USBDriveLetter = ""
                        USBDriveName = ""
                    End If
                Else
                    MsgBox(obj("InterfaceType"))
                End If
            Case Else
                MsgBox("Unknown error: " & obj("Caption"))
        End Select
    End Sub

    Private Function GetDriveLetterFromDisk(ByVal Name As String) As String
        Dim oq_part, oq_disk As ObjectQuery
        Dim mos_part, mos_disk As ManagementObjectSearcher
        Dim obj_part, obj_disk As ManagementObject
        Dim ans As String = ""

        ' WMI queries use the "\" as an escape charcter
        Name = Replace(Name, "\", "\\")

        ' First we map the Win32_DiskDrive instance with the association called
        ' Win32_DiskDriveToDiskPartition. Then we map the Win23_DiskPartion
        ' instance with the assocation called Win32_LogicalDiskToPartition

        oq_part = New ObjectQuery("ASSOCIATORS OF {Win32_DiskDrive.DeviceID=""" & Name & """} WHERE AssocClass = Win32_DiskDriveToDiskPartition")
        mos_part = New ManagementObjectSearcher(oq_part)
        For Each obj_part In mos_part.Get()

            oq_disk = New ObjectQuery("ASSOCIATORS OF {Win32_DiskPartition.DeviceID=""" & obj_part("DeviceID") & """} WHERE AssocClass = Win32_LogicalDiskToPartition")
            mos_disk = New ManagementObjectSearcher(oq_disk)
            For Each obj_disk In mos_disk.Get()
                ans &= obj_disk("Name") & ","
            Next
        Next

        Return ans.Trim(","c)
    End Function

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        StartDetection()
    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        m_MediaConnectWatcher.Stop()
        Button1.Enabled = False
        Button2.Enabled = True
        Button2.Text = "Start Auto Polling"

    End Sub

    Private Sub Button1_Click_1(sender As Object, e As EventArgs) Handles Button1.Click

    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        m_MediaConnectWatcher.Start()
        Button2.Enabled = False
        Button1.Enabled = True
    End Sub
End Class
