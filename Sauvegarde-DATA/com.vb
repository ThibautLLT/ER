Imports System.IO
Imports System.IO.File


Public Class com
    '------------------------------------------------
    Dim myPort As Array
    Dim ComOpen
    Dim ComOpen2
    Dim d As DateTime
    Dim dateDuJour As String
    Dim nomFichier As String
    Dim workSpace As String

    Dim cache As String = ""

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
            ComboBox3.Items.AddRange(myPort) 'Ajout des ports à la combobox
            ComboBox3.Text = ComboBox3.Items(ComboBox3.Items.Count - 1)
            ComboBox4.Text = "115200" 'Ajout du bitrate
            SerialPort1.PortName = ComboBox3.Text
            SerialPort1.BaudRate = ComboBox4.Text
            SerialPort1.Open()
            ComOpen = True
        Catch ex As Exception
        End Try

        'timer interval
        Dim Timer As New Timers.Timer(900000) '1000 ms '900000 pour 15 minutes
        Timer.AutoReset = True
        AddHandler Timer.Elapsed, AddressOf TimerElapsedHandler
        Timer.Start()

    End Sub



    Private Sub SerialPort1_DataReceived(sender As System.Object, e As System.IO.Ports.SerialDataReceivedEventArgs) Handles SerialPort1.DataReceived
        Dim textRecu = SerialPort1.ReadExisting() 'Reception des messages
        analyseChaine(textRecu)
    End Sub

    Private Sub com_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        If ComOpen = True Then
            SerialPort1.Close()
        End If
        If ComOpen2 = True Then
            SerialPort2.Close()
        End If
    End Sub

    Sub analyseChaine(data As String)

        cache += data.Replace(vbLf, "")
        Dim i As Integer = 0
        Dim splitedCache As Array = cache.Split(vbCr)

        For Each splited As String In splitedCache
            i = i + 1
            If i < splitedCache.Length Then
                Dim time As String = Now.ToLongTimeString()
                editionChaine(time + " " + splited)
            Else
                cache = splited
            End If

        Next
    End Sub
    Private Sub editionChaine(data As String)
        ajoutAuSvG(data.Replace(" ", ","))
    End Sub

    Private Sub ajoutAuSvG(data As String)
        Dim filename As String = My.Application.Info.DirectoryPath & "\log_data\" + Now.ToShortDateString().Replace("/", "_") + ".csv"
        Dim sw As StreamWriter = AppendText(filename)
        sw.WriteLine(data)
        sw.Close()
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        myPort = IO.Ports.SerialPort.GetPortNames()
        ComboBox5.Items.AddRange(myPort) 'Ajout des ports à la combobox
        ComboBox5.Text = ComboBox5.Items(ComboBox5.Items.Count - 1)
        ComboBox6.Text = "115200" 'Ajout du bitrate

        SerialPort2.PortName = ComboBox5.Text
        SerialPort2.BaudRate = ComboBox6.Text
        SerialPort2.Open()
        ComOpen2 = True
    End Sub
    Private Sub TimerElapsedHandler(ByVal sender As Object, ByVal e As Timers.ElapsedEventArgs)

        'request tension value on timer event
        If ComOpen2 = True Then
            SerialPort2.WriteLine("P") ' envoie de la demande de tensions
        End If
    End Sub
End Class