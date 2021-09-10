Imports System
Imports System.Threading
Imports System.IO.Ports
Imports System.ComponentModel
Imports System.String
Imports Microsoft.Office.Interop
Imports Microsoft.Office.Interop.Excel
Imports System.IO
Imports System.IO.File


Public Class com
    '------------------------------------------------
    Dim myPort As Array
    Dim ComOpen
    Dim d As DateTime
    Dim dateDuJour As String
    Dim nomFichier As String
    Dim workSpace As String

    Dim cacheData As String

    ' objet OLE pour controle Excel
    Dim ApExcel As New Excel.Application
    Dim objBooks As Excel.Workbooks
    Dim objBook As Excel._Workbook
    Dim objSheets As Excel.Sheets
    Dim objSheet As Excel._Worksheet



    Delegate Sub SetTextCallback(ByVal [text] As String) 'Added to prevent threading errors during receiveing of data
    '------------------------------------------------
    Private Sub Form1_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load
        'récupération du répertoire de sauvegarde
        'RichTextBox3.Text = My.Settings.Folder

        'Récupération de la date du jour
        d = Now
        dateDuJour = d.ToShortDateString.Replace("/", "_") 'formatage de la date du jour

        'Connexion au port série
        Try
            myPort = IO.Ports.SerialPort.GetPortNames()
            ComboBox1.Items.AddRange(myPort) 'Ajout des ports à la combobox
            ComboBox1.Text = ComboBox1.Items(ComboBox1.Items.Count - 1)
            ComboBox2.Text = "115200" 'Ajout du bitrate
            SerialPort1.PortName = ComboBox1.Text
            SerialPort1.BaudRate = ComboBox2.Text
            SerialPort1.Open()
            ComOpen = True
        Catch ex As Exception
        End Try
    End Sub



    Private Sub SerialPort1_DataReceived(sender As System.Object, e As System.IO.Ports.SerialDataReceivedEventArgs) Handles SerialPort1.DataReceived
        Dim textRecu = SerialPort1.ReadExisting() 'Reception des messages
        sauvegarde_data(textRecu)

    End Sub

    Private Sub com_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        If ComOpen = True Then
            SerialPort1.Close()
        End If
    End Sub

    Public Sub sauvegarde_data(data As String)
        Dim filename As String = My.Application.Info.DirectoryPath & "\log_data\" + Now.ToShortDateString().Replace("/", "_") + ".svg"
        Dim sw As StreamWriter = AppendText(filename)
        Dim csvFormat As String
        cacheData += data



        csvFormat = csvFormat.Replace(" ", ",")
        'sw.WriteLine(csvFormat)
        'sw.Close()
        cacheData = ""
    End Sub

End Class