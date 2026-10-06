Option Explicit On
Option Strict On
Option Infer On

Imports System

Public NotInheritable Class HeatIndexResult
    Public ReadOnly TemperatureFahrenheit As Double
    Public ReadOnly UsesRothfuszRegression As Boolean
    Public ReadOnly AdjustmentFahrenheit As Double

    Public Sub New(temperatureFahrenheit As Double, usesRothfuszRegression As Boolean, adjustmentFahrenheit As Double)
        Me.TemperatureFahrenheit = temperatureFahrenheit
        Me.UsesRothfuszRegression = usesRothfuszRegression
        Me.AdjustmentFahrenheit = adjustmentFahrenheit
    End Sub
End Class

''' <summary>Formula implementations used by the temperature and humidity page.</summary>
Public NotInheritable Class ThermalCalculator
    Private Sub New()
    End Sub

    ''' <summary>
    ''' CWA's shaded-outdoor Steadman apparent-temperature approximation.
    ''' Temperature is in Celsius, relative humidity in percent, and wind in m/s.
    ''' </summary>
    Public Shared Function CalculateSteadmanApparentTemperature(temperatureCelsius As Double,
                                                                 relativeHumidityPercent As Double,
                                                                 windSpeedMetersPerSecond As Double) As Double
        Dim vaporPressureHpa As Double = relativeHumidityPercent / 100.0 *
            6.105 * Math.Exp((17.27 * temperatureCelsius) / (237.7 + temperatureCelsius))
        Return 1.04 * temperatureCelsius + 0.2 * vaporPressureHpa -
            0.65 * windSpeedMetersPerSecond - 2.7
    End Function

    ''' <summary>
    ''' Applies the NOAA screening equation and Rothfusz regression, including the low/high RH adjustments.
    ''' The returned value and adjustment are in Fahrenheit.
    ''' </summary>
    Public Shared Function CalculateHeatIndex(temperatureFahrenheit As Double,
                                               relativeHumidityPercent As Double) As HeatIndexResult
        Dim simpleFormula As Double = 0.5 * (temperatureFahrenheit + 61.0 +
            ((temperatureFahrenheit - 68.0) * 1.2) + (relativeHumidityPercent * 0.094))
        Dim simpleEstimate As Double = (simpleFormula + temperatureFahrenheit) / 2.0

        If simpleEstimate < 80.0 Then
            Return New HeatIndexResult(simpleEstimate, False, 0.0)
        End If

        Dim t As Double = temperatureFahrenheit
        Dim rh As Double = relativeHumidityPercent
        Dim t2 As Double = t * t
        Dim rh2 As Double = rh * rh
        Dim heatIndex As Double = -42.379 + 2.04901523 * t + 10.14333127 * rh -
            0.22475541 * t * rh - 0.00683783 * t2 - 0.05481717 * rh2 +
            0.00122874 * t2 * rh + 0.00085282 * t * rh2 - 0.00000199 * t2 * rh2

        Dim adjustment As Double = 0.0
        If rh < 13.0 AndAlso t >= 80.0 AndAlso t <= 112.0 Then
            adjustment = -((13.0 - rh) / 4.0) * Math.Sqrt(Math.Max(0.0, (17.0 - Math.Abs(t - 95.0)) / 17.0))
        ElseIf rh > 85.0 AndAlso t >= 80.0 AndAlso t <= 87.0 Then
            adjustment = ((rh - 85.0) / 10.0) * ((87.0 - t) / 5.0)
        End If

        Return New HeatIndexResult(heatIndex + adjustment, True, adjustment)
    End Function

    ''' <summary>Converts air temperature and dew point to relative humidity using Magnus-Tetens (a=17.625, b=243.04 C).</summary>
    Public Shared Function RelativeHumidityFromDewPoint(temperatureCelsius As Double,
                                                         dewPointCelsius As Double) As Double
        Const magnusA As Double = 17.625
        Const magnusB As Double = 243.04
        Dim humidity As Double = 100.0 * Math.Exp(
            magnusA * dewPointCelsius / (magnusB + dewPointCelsius) -
            magnusA * temperatureCelsius / (magnusB + temperatureCelsius))
        Return Math.Max(0.0, Math.Min(100.0, humidity))
    End Function

    Public Shared Function CelsiusToFahrenheit(temperatureCelsius As Double) As Double
        Return temperatureCelsius * 9.0 / 5.0 + 32.0
    End Function

    Public Shared Function FahrenheitToCelsius(temperatureFahrenheit As Double) As Double
        Return (temperatureFahrenheit - 32.0) * 5.0 / 9.0
    End Function
End Class
