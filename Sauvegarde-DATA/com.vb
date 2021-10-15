Imports System.IO
Imports System.IO.File
Imports System.Web.script.serialisation


Public Class com
    '------------------------------------------------
    Dim myPort As Array
    Dim ComOpen
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
        analyseChaine(textRecu)
    End Sub

    Private Sub com_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        If ComOpen = True Then
            SerialPort1.Close()
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
                Dim mots As Array = splited.Split(" ") 'divise la chaine de carractère dans un tableau, id du message à la case 1
                Select Case mots(1)
                    Case "51" ' Message ID 51 data0 3 data1 5
                        Console.WriteLine(mots(3))

                    Case "32" ' Etat des relais
                    Case "31" ' temp panneau
                    Case "51" ' Irradiance
                    Case "71" ' ?
                    Case Else
                        ' default 
                End Select

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

End Class
