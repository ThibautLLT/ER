Public Class FormTerminale
    Public Property StringPass As String
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        RichTextBox1.Text = "" 'suppression du text
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Envoyer_Serie(TextBox1.Text) ' commande envoyé 
    End Sub

    Private Sub TextBox1_Click(sender As Object, e As EventArgs) Handles TextBox1.Click
        TextBox1.Text = "" 'suppression du text
    End Sub

    ' Envoyer la commande à la carte
    Function Envoyer_Serie(text)
        Try
            FormLog.EcrireLog("Commande envoyé dans le teminale : " + text)
            RichTextBox1.Text += text & vbCr
            com.SerialPort1.Write(text & vbCr)
            TextBox1.Text = ""
        Catch ex As Exception
            FormLog.EcrireLog(ex.ToString())
        End Try

    End Function

End Class