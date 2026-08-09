Public Class 氣象小工具v1

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim C, F As Single
        C = TextBox1.Text
        F = 9 / 5 * C + 32
        TextBox2.Text = F

    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Dim C, F As Double
        F = TextBox4.Text
        C = 5 / 9 * (F - 32)
        TextBox3.Text = C
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        Dim W, U, I, H As Double
        W = TextBox5.Text
        U = W * 1.852
        I = W * 0.514
        H = W * 1.15077945
        TextBox6.Text = U
        TextBox7.Text = I
        TextBox8.Text = H
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        If TextBox5.Text <= 33 Then
            Label11.Text = "熱帶低氣壓"
        ElseIf TextBox5.Text < 64 Then
            Label11.Text = "熱帶風暴"
        ElseIf TextBox5.Text < 130 Then
            Label11.Text = "颱風"
        Else
            Label11.Text = "超級颱風"
        End If
        If TextBox7.Text <= 17.1 Then
            Label12.Text = "熱帶低氣壓"
        ElseIf TextBox7.Text < 32.6 Then
            Label12.Text = "輕度颱風"
        ElseIf TextBox7.Text < 50.9 Then
            Label12.Text = "中度颱風"
        Else
            Label12.Text = "強烈颱風"
        End If
        If TextBox7.Text <= 17 Then
            Label16.Text = "熱帶低氣壓"
        ElseIf TextBox7.Text < 33 Then
            Label16.Text = "台風"
        ElseIf TextBox7.Text < 44 Then
            Label16.Text = "強い台風"
        ElseIf TextBox7.Text < 54 Then
            Label16.Text = "非常に強い台風"
        Else
            Label16.Text = "猛烈な台風"
        End If
        If TextBox6.Text <= 62 Then
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
        If TextBox8.Text <= 38 Then
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

End Class
