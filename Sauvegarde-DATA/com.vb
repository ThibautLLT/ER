Imports System
Imports System.Threading
Imports System.IO.Ports
Imports System.ComponentModel
Imports System.String
Imports Microsoft.Office.Interop
Imports Microsoft.Office.Interop.Excel


Public Class com
    '------------------------------------------------
    Dim myPort As Array
    Dim textATraiter As String
    Dim textAffiche As String
    Dim folder As String            'chemin accès fichier
    Dim ComOpen
    Dim d As DateTime
    Dim dateDuJour As String
    Dim nomFichier As String
    Dim workSpace As String

    ' objet OLE pour controle Excel
    Dim ApExcel As New Excel.Application
    Dim objBooks As Excel.Workbooks
    Dim objBook As Excel._Workbook
    Dim objSheets As Excel.Sheets
    Dim objSheet As Excel._Worksheet
    Dim numLigne = 0

    Delegate Sub SetTextCallback(ByVal [text] As String) 'Added to prevent threading errors during receiveing of data
    '------------------------------------------------
    Private Sub Form1_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load
        'récupération du répertoire de sauvegarde
        RichTextBox3.Text = My.Settings.Folder

        'fermeture des processus Exel
        FormLog.EcrireLog("Fermeture du processus EXCEL")
        For Each P As Process In System.Diagnostics.Process.GetProcessesByName("EXCEL")
            P.Kill()
        Next

        'Récupération de la date du jour
        d = Now
        dateDuJour = d.ToShortDateString.Replace("/", "_") 'formatage de la date du jour

        'Connexion au port série
        Try
            FormLog.EcrireLog("Connection au port série")
            myPort = IO.Ports.SerialPort.GetPortNames()
            ComboBox1.Items.AddRange(myPort) 'Ajout des ports à la combobox
            ComboBox1.Text = ComboBox1.Items(ComboBox1.Items.Count - 1)
            ComboBox2.Text = "115200" 'Ajout du bitrate
            SerialPort1.PortName = ComboBox1.Text
            SerialPort1.BaudRate = ComboBox2.Text
            SerialPort1.Open()
            ComOpen = True
        Catch ex As Exception
            FormLog.EcrireLog(ex.ToString())
        End Try

        Try
            FormLog.EcrireLog("Lancement d'excel")
            ApExcel = CreateObject("Excel.Application") 'nouvel Objet ApExcel
            ApExcel.Visible = True 'Permet de rendre visible ou non la fenêtre Excel

            workSpace = My.Application.Info.DirectoryPath 'repertoire de travail
            objBooks = ApExcel.Workbooks

            objBook = objBooks.Open(workSpace + "\acquisitionOK.xlsm")             ' open acquisition excel 
            My.Computer.FileSystem.DeleteFile(workSpace + "\acquisition.xlsm")     ' delete acquisition excel file before
            objBook.SaveAs(workSpace + "\acquisition.xlsm")                        ' Save as  excel file as acquistion 
            objBook.Close()

            objBooks = ApExcel.Workbooks
            objBook = objBooks.Open(workSpace + "\acquisition.xlsm")
            objSheets = objBook.Worksheets

            ApExcel.Run("Bouton1_Cliquer") 'lancement de la macro Bouton1_cliquer, pour lancer une acquisition
        Catch ex As Exception
            FormLog.EcrireLog(ex.ToString())
        End Try

        Timer1.Start()
        FormLog.EcrireLog("Programme lancé")
    End Sub



    Private Sub SerialPort1_DataReceived(sender As System.Object, e As System.IO.Ports.SerialDataReceivedEventArgs) Handles SerialPort1.DataReceived
        Dim textRecu = SerialPort1.ReadExisting() 'Reception des messages
        ReceivedText(textRecu) 'traitement du text reçu
    End Sub

    Private Sub ReceivedText(ByVal [text] As String) 'input from ReadExisting
        'traitement du texte reçu
        If Me.RichTextBox2.InvokeRequired Then
            Dim x As New SetTextCallback(AddressOf ReceivedText)
            Me.Invoke(x, New Object() {(text)})
        Else
            Me.TextBox2.Text = text
            FormTerminale.RichTextBox1.Text = FormTerminale.RichTextBox1.Text + text
            analyseChaine() 'appelle de l'analyse de la chaine
        End If

    End Sub



    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        Me.RichTextBox2.Text = "" 'remise à zero du text de la richtextBox
    End Sub

    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        Me.RichTextBox1.Text = "" 'remise à zero du text de la richtextBox
    End Sub

    Private Sub com_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        If ComOpen = True Then
            SerialPort1.Close()
        End If
    End Sub


    Private Sub analyseChaine()                 'Traiter chaine des données 
        Dim textRecu As String
        Dim ch As Char

        textRecu = TextBox2.Text
        For i = 0 To Len(textRecu) - 1
            ch = textRecu(i)
            If Asc(ch) <> 13 Then
                If Asc(ch) > 31 And Asc(ch) < 123 Then
                    textATraiter &= ch
                End If
            Else
                objSheet = objBook.Sheets("Reception")
                RichTextBox2.Text = textATraiter & vbCr
                numLigne = numLigne + 1
                objSheet.Cells(1, 1) = textATraiter
                objSheet.Cells(3 + numLigne, 1) = textATraiter
                ApExcel.Run("Macro1")
                textATraiter = ""
            End If
        Next
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        Dim commande As String

        '   objBook.Save()            'save every second 
        Timer1.Stop()
        d = Now
        If Compare(dateDuJour, d.ToShortDateString.Replace("/", "_")) <> 0 Then           '<> différent 

            ' appelle de la fonction de sauvegarde 
            Sauvegarde("Sauvegarde_du_" + d.ToShortDateString.Replace("/", "_"), RichTextBox3.Text)

            ' actualisation de la date du jour
            dateDuJour = d.ToShortDateString.Replace("/", "_")

        End If

        ' sélection de la page Emission
        objSheet = objBook.Sheets("Emission")
        commande = objSheet.Cells(1, 1).value
        If commande <> "" Then
            RichTextBox1.Text &= commande & vbCr
            If SerialPort1.IsOpen Then
                SerialPort1.Write(commande & vbCr) 'concatenate with #13
            End If
            objSheet.Cells(1, 1).value = ""
        End If
        Timer1.Start()
    End Sub

    ' sauvegardes ponctuelles
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Try
            FormLog.EcrireLog("Bouton sauvegarde appuyé")
            Dim quote As String = """"""
            quote = quote.Substring(1)
            ' suppression des carractères interdits 
            Dim Name As String = RichTextBox4.Text.Replace("\", "_").Replace("/", "_").Replace(":", "_").Replace("*", "_").Replace("?", "_").Replace(quote, "_").Replace("<", "_").Replace(">", "_").Replace("|", "_")
            Dim folder As String = RichTextBox3.Text

            RichTextBox4.Text = Name
            RichTextBox3.Text = folder
        Catch ex As Exception
            FormLog.EcrireLog(ex.ToString())
        End Try

        Sauvegarde(RichTextBox4.Text, RichTextBox3.Text)
    End Sub

    ' fonction de sauvegegarde sauvegarde 
    ' name de type string, nom de la sauvegarde
    ' folder de type string,  répertoire de sauvegarde
    Function Sauvegarde(name, folder) As Tasks.Task
        Dim fso
        fso = CreateObject("Scripting.FileSystemObject")

        Try
            FormLog.EcrireLog("Sauvegarde...")
            If fso.FolderExists(folder) Then
                If fso.FileExists(folder + "\" + name + ".xlsm") Or Compare(name, "") <> 1 Then
                    MessageBox.Show("Nom de Fichier Invalide, le fichier éxiste déjà.")
                Else
                    d = Now
                    nomFichier = folder + "\" + name + ".xlsm"
                    objBook.SaveAs(nomFichier)
                    objBook.Close()

                    objBook = objBooks.Open(workSpace + "\acquisitionOK.xlsm")

                    My.Computer.FileSystem.DeleteFile(workSpace + "\acquisition.xlsm")

                    objBook.SaveAs(workSpace + "\acquisition.xlsm")

                    objBook.Close()

                    For Each P As Process In System.Diagnostics.Process.GetProcessesByName("EXCEL")
                        P.Kill()
                    Next

                    ApExcel = CreateObject("Excel.Application") 'nouvel Objet ApExcel
                    ApExcel.Visible = True 'Permet de rendre visible ou non la fenêtre Excel

                    workSpace = My.Application.Info.DirectoryPath 'repertoire de travail
                    objBooks = ApExcel.Workbooks
                    objBook = objBooks.Open(workSpace + "\acquisition.xlsm")
                    objSheets = objBook.Worksheets

                    dateDuJour = d.ToShortDateString.Replace("/", "_")


                    objSheet = objBook.Sheets("Emission")

                    ApExcel.Run("Bouton1_Cliquer")
                End If
            Else
                FormLog.EcrireLog("dossier non accessible")
                MessageBox.Show("Ce dossier n'est pas accessible depuis cette machine.")
            End If
        Catch ex As Exception
            FormLog.EcrireLog(ex.ToString())
        End Try

    End Function

    ' lancement d'une acquisition
    Private Sub Acquisition_Click(sender As Object, e As EventArgs) Handles Acquisition.Click
        ApExcel.Run("Bouton1_Cliquer")
        FormLog.EcrireLog("Acquisition lancée")
    End Sub

    ' affichage de la fenetre du terminale
    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        FormTerminale.Show()
    End Sub

    'affichage de la fenetre des logs
    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        FormLog.Show()
    End Sub

    'fermeture du programme
    Private Sub Form1_FormClosing(ByVal sender As Object,
        ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        ' sauvegarde du repertoire de sauvegarde dans les parametres
        My.Settings.Folder = RichTextBox3.Text
        My.Settings.Save()
    End Sub

    Private Sub Button6_Click(sender As Object, e As EventArgs) Handles Button6.Click
        If (FolderBrowserDialog1.ShowDialog() = DialogResult.OK) Then
            RichTextBox3.Text = FolderBrowserDialog1.SelectedPath
        End If

    End Sub

    ' vérification shutdown
    Private Sub Button8_Click(sender As Object, e As EventArgs) Handles Button8.Click
        SerialPort1.WriteLine("V")
    End Sub
End Class