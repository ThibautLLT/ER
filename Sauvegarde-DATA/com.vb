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
        editionChaine(textRecu) 'Ajout au cache du text
    End Sub

    Private Sub com_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        If ComOpen = True Then
            SerialPort1.Close()
        End If
    End Sub
    Private Sub editionChaine(data As String)
        ajoutAuSvG(data.Replace(" ", ","))
    End Sub
    Private Sub ajoutAuSvG(data As String)
        Dim filename As String = My.Application.Info.DirectoryPath & "\log_data\" + Now.ToShortDateString().Replace("/", "_") + ".csv"
        Dim sw As StreamWriter = AppendText(filename)
        sw.Write(data)
        sw.Close()
    End Sub

End Class