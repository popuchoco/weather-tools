Public Class 氣象小工具v1
    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        Dim W, U, I, H As Double
        W = TextBox5.Text
        U = W * 1.852
        I = W * 0.514
        H = W * 1.15077945
        TextBox6.Text = U
        TextBox7.Text = I
        TextBox8.Text = H
        If TextBox5.Text < 22 Then
            Label11.Text = "-"
        ElseIf TextBox5.Text <= 33 Then
            Label11.Text = "熱帶低氣壓"
        ElseIf TextBox5.Text < 64 Then
            Label11.Text = "熱帶風暴"
        ElseIf TextBox5.Text < 130 Then
            Label11.Text = "颱風"
        Else
            Label11.Text = "超級颱風"
        End If
        If TextBox7.Text < 10.8 Then
            Label12.Text = "-"
        ElseIf TextBox7.Text <= 17.1 Then
            Label12.Text = "熱帶低氣壓"
        ElseIf TextBox7.Text < 32.6 Then
            Label12.Text = "輕度颱風"
        ElseIf TextBox7.Text < 50.9 Then
            Label12.Text = "中度颱風"
        Else
            Label12.Text = "強烈颱風"
        End If
        If TextBox7.Text < 10.8 Then
            Label16.Text = "-"
        ElseIf TextBox7.Text <= 17 Then
            Label16.Text = "熱帶低氣壓"
        ElseIf TextBox7.Text < 24.4 Then
            Label16.Text = "熱帶風暴"
        ElseIf TextBox7.Text < 32.6 Then
            Label16.Text = "強烈熱帶風暴"
        ElseIf TextBox7.Text < 44 Then
            Label16.Text = "強い台風"
        ElseIf TextBox7.Text < 54 Then
            Label16.Text = "非常に強い台風"
        Else
            Label16.Text = "猛烈な台風"
        End If
        If TextBox6.Text < 41 Then
            Label15.Text = "-"
        ElseIf TextBox6.Text <= 62 Then
            Label15.Text = "熱帶低氣壓"
        ElseIf TextBox6.Text < 87 Then
            Label15.Text = "熱帶風暴"
        ElseIf TextBox6.Text < 117 Then
            Label15.Text = "強烈熱帶風暴"
        ElseIf TextBox6.Text < 149 Then
            Label15.Text = "颱風"
        ElseIf TextBox6.Text < 184 Then
            Label15.Text = "強颱風"
        Else
            Label15.Text = "超強颱風"
        End If
        If TextBox8.Text < 25 Then
            Label24.Text = "-"
        ElseIf TextBox8.Text <= 38 Then
            Label24.Text = "熱帶低氣壓"
        ElseIf TextBox8.Text < 73 Then
            Label24.Text = "熱帶風暴"
        ElseIf TextBox8.Text < 95 Then
            Label24.Text = "一級颶風"
        ElseIf TextBox8.Text < 110 Then
            Label24.Text = "二級颶風"
        ElseIf TextBox8.Text < 129 Then
            Label24.Text = "三級颶風"
        ElseIf TextBox8.Text < 156 Then
            Label24.Text = "四級颶風"
        Else
            Label24.Text = "五級颶風"
        End If
    End Sub
    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        Dim V, B As Single
        B = TextBox9.Text

        V = 0.836 * (B ^ (3 / 2))
        TextBox10.Text = V
    End Sub


    Private Sub Button7_Click(sender As Object, e As EventArgs) Handles Button7.Click
        Dim G, H As Double
        H = TextBox13.Text

        G = 0.154 * (1019 - H)
        TextBox14.Text = G
    End Sub

    Private Sub 氣象小工具v1_Load(sender As Object, e As EventArgs) Handles Me.Load
        Dim wr As Net.WebRequest = Net.WebRequest.Create("http://www.cwb.gov.tw/V7/observe/satellite/Data/s1p/s1p.jpg")
        Dim res As Net.WebResponse = wr.GetResponse
        Dim bmp As New Bitmap(res.GetResponseStream)
        PictureBox1.Image = bmp
    End Sub
End Class
