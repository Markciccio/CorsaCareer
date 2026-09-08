param(
    [string]$OutputDirectory = (Join-Path $PSScriptRoot "..\assets\ui-icons")
)

Add-Type -AssemblyName System.Drawing

$resolvedOutput = [System.IO.Path]::GetFullPath($OutputDirectory)
[System.IO.Directory]::CreateDirectory($resolvedOutput) | Out-Null

function New-IconCanvas {
    param([string]$Name, [System.Drawing.Color]$Accent, [scriptblock]$Draw)

    # Disegno a 2x e riduzione finale: i dettagli restano puliti anche nel
    # riquadro Oggi, dove l'icona viene visualizzata a circa 70 pixel.
    $largeBitmap = New-Object System.Drawing.Bitmap 256, 256
    $graphics = [System.Drawing.Graphics]::FromImage($largeBitmap)
    $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $graphics.Clear([System.Drawing.Color]::Transparent)
    $graphics.ScaleTransform(2.0, 2.0)

    $shield = New-Object System.Drawing.Drawing2D.GraphicsPath
    $shield.AddPolygon(@(
        [System.Drawing.PointF]::new(64, 4),
        [System.Drawing.PointF]::new(106, 18),
        [System.Drawing.PointF]::new(119, 52),
        [System.Drawing.PointF]::new(109, 94),
        [System.Drawing.PointF]::new(64, 122),
        [System.Drawing.PointF]::new(19, 94),
        [System.Drawing.PointF]::new(9, 52),
        [System.Drawing.PointF]::new(22, 18)
    ))
    $shadowPath = $shield.Clone()
    $shadowMatrix = New-Object System.Drawing.Drawing2D.Matrix
    $shadowMatrix.Translate(0, 4)
    $shadowPath.Transform($shadowMatrix)

    $shadow = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(125, 0, 0, 0))
    $surface = New-Object System.Drawing.Drawing2D.LinearGradientBrush (
        [System.Drawing.RectangleF]::new(8, 5, 112, 117),
        [System.Drawing.Color]::FromArgb(255, 36, 43, 56),
        [System.Drawing.Color]::FromArgb(255, 10, 13, 20),
        62.0
    )
    $glow = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(55, $Accent.R, $Accent.G, $Accent.B)), 11
    $ring = New-Object System.Drawing.Pen $Accent, 4.2
    $ring.LineJoin = [System.Drawing.Drawing2D.LineJoin]::Round
    $innerRing = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(145, $Accent.R, $Accent.G, $Accent.B)), 1.2
    $techPen = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(55, 225, 232, 242)), 1
    $accentFine = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(210, $Accent.R, $Accent.G, $Accent.B)), 2
    $accentFine.StartCap = $accentFine.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
    $white = [System.Drawing.Color]::FromArgb(245, 247, 250)
    $symbolShadow = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(170, 0, 0, 0)), 10
    $symbolShadow.StartCap = $symbolShadow.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
    $symbolShadow.LineJoin = [System.Drawing.Drawing2D.LineJoin]::Round
    $pen = New-Object System.Drawing.Pen $white, 5.5
    $pen.StartCap = $pen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
    $pen.LineJoin = [System.Drawing.Drawing2D.LineJoin]::Round
    $fill = New-Object System.Drawing.SolidBrush $white
    $accentFill = New-Object System.Drawing.SolidBrush $Accent
    $darkFill = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(255, 13, 17, 24))
    try {
        $graphics.FillPath($shadow, $shadowPath)
        $graphics.FillPath($surface, $shield)
        $graphics.DrawPath($glow, $shield)
        $graphics.DrawPath($ring, $shield)

        # Trama tecnica, come una piccola piastra strumenti del paddock.
        $graphics.DrawLine($techPen, 25, 31, 103, 31)
        $graphics.DrawLine($techPen, 20, 88, 108, 88)
        $graphics.DrawLine($techPen, 34, 19, 34, 105)
        $graphics.DrawLine($techPen, 94, 22, 94, 101)
        $graphics.DrawEllipse($innerRing, 20, 17, 88, 88)
        $graphics.FillEllipse($accentFill, 25, 25, 4, 4)
        $graphics.FillEllipse($accentFill, 99, 25, 4, 4)
        $graphics.FillEllipse($accentFill, 25, 91, 4, 4)
        $graphics.FillEllipse($accentFill, 99, 91, 4, 4)

        # Doppia V inferiore: richiama una patch di categoria motorsport.
        $graphics.DrawLines($accentFine, @(
            [System.Drawing.Point]::new(48, 108),
            [System.Drawing.Point]::new(64, 116),
            [System.Drawing.Point]::new(80, 108)
        ))
        $graphics.DrawLines($techPen, @(
            [System.Drawing.Point]::new(52, 103),
            [System.Drawing.Point]::new(64, 109),
            [System.Drawing.Point]::new(76, 103)
        ))

        # Ombra del pittogramma, poi simbolo principale e accenti specifici.
        $graphics.TranslateTransform(1.5, 2.0)
        & $Draw $graphics $symbolShadow $darkFill $Accent
        $graphics.TranslateTransform(-1.5, -2.0)
        & $Draw $graphics $pen $fill $Accent

        # Piccolo indicatore di stato in alto, leggibile senza testo.
        $graphics.FillEllipse($darkFill, 56, 8, 16, 16)
        $graphics.DrawEllipse($accentFine, 57, 9, 14, 14)
        $graphics.FillEllipse($accentFill, 61, 13, 6, 6)

        $bitmap = New-Object System.Drawing.Bitmap 128, 128
        $finalGraphics = [System.Drawing.Graphics]::FromImage($bitmap)
        $finalGraphics.Clear([System.Drawing.Color]::Transparent)
        $finalGraphics.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality
        $finalGraphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
        $finalGraphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
        $finalGraphics.DrawImage($largeBitmap, 0, 0, 128, 128)
        $path = Join-Path $resolvedOutput ("oggi-{0}.png" -f $Name)
        $bitmap.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
        $finalGraphics.Dispose()
        $bitmap.Dispose()
    }
    finally {
        $darkFill.Dispose(); $accentFill.Dispose(); $fill.Dispose(); $pen.Dispose()
        $symbolShadow.Dispose(); $accentFine.Dispose(); $techPen.Dispose(); $innerRing.Dispose()
        $ring.Dispose(); $glow.Dispose(); $surface.Dispose(); $shadow.Dispose()
        $shadowMatrix.Dispose(); $shadowPath.Dispose(); $shield.Dispose()
        $graphics.Dispose(); $largeBitmap.Dispose()
    }
}

$red = [System.Drawing.Color]::FromArgb(235, 30, 67)
$amber = [System.Drawing.Color]::FromArgb(245, 183, 51)
$green = [System.Drawing.Color]::FromArgb(48, 196, 128)
$blue = [System.Drawing.Color]::FromArgb(70, 155, 245)
$cyan = [System.Drawing.Color]::FromArgb(54, 201, 218)
$purple = [System.Drawing.Color]::FromArgb(161, 112, 255)

# Cronometro: test e prove di valutazione.
New-IconCanvas "test" $amber {
    param($g, $p, $b, $a)
    $g.DrawEllipse($p, 36, 38, 56, 56)
    $g.DrawLine($p, 64, 38, 64, 29)
    $g.DrawLine($p, 53, 28, 75, 28)
    $g.DrawLine($p, 64, 66, 79, 52)
    $g.FillEllipse($b, 59, 61, 10, 10)
}

# Bandiera a scacchi: gara singola o su invito.
New-IconCanvas "gara" $red {
    param($g, $p, $b, $a)
    $g.DrawLine($p, 39, 29, 39, 96)
    $g.DrawLines($p, @([System.Drawing.Point]::new(43,34), [System.Drawing.Point]::new(88,34), [System.Drawing.Point]::new(77,59), [System.Drawing.Point]::new(43,59)))
    $dark = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(255,24,29,38))
    try {
        $g.FillRectangle($b, 44, 35, 15, 12); $g.FillRectangle($b, 73, 35, 14, 12)
        $g.FillRectangle($b, 59, 47, 15, 11)
        $g.FillRectangle($dark, 59, 35, 14, 12); $g.FillRectangle($dark, 44, 47, 15, 11); $g.FillRectangle($dark, 74, 47, 10, 8)
    } finally { $dark.Dispose() }
}

# Coppa: round di campionato.
New-IconCanvas "campionato" $amber {
    param($g, $p, $b, $a)
    $g.DrawRectangle($p, 45, 30, 38, 38)
    $g.DrawArc($p, 29, 35, 32, 31, 90, 180)
    $g.DrawArc($p, 67, 35, 32, 31, 270, 180)
    $g.DrawLine($p, 64, 69, 64, 87)
    $g.DrawLine($p, 48, 91, 80, 91)
}

# Manubrio: allenamento e preparazione fisica.
New-IconCanvas "allenamento" $green {
    param($g, $p, $b, $a)
    $g.DrawLine($p, 38, 64, 90, 64)
    $g.DrawLine($p, 34, 49, 34, 79); $g.DrawLine($p, 43, 44, 43, 84)
    $g.DrawLine($p, 85, 44, 85, 84); $g.DrawLine($p, 94, 49, 94, 79)
}

# Stretta di mano stilizzata: ricerca e accordo sponsor.
New-IconCanvas "sponsor" $blue {
    param($g, $p, $b, $a)
    $g.DrawLines($p, @([System.Drawing.Point]::new(29,52),[System.Drawing.Point]::new(47,39),[System.Drawing.Point]::new(63,50),[System.Drawing.Point]::new(79,39),[System.Drawing.Point]::new(99,54)))
    $g.DrawLines($p, @([System.Drawing.Point]::new(34,67),[System.Drawing.Point]::new(52,84),[System.Drawing.Point]::new(64,74),[System.Drawing.Point]::new(77,84),[System.Drawing.Point]::new(94,67)))
    $g.DrawLine($p, 52, 55, 76, 72)
}

# Fumetto con cuore: social, tifosi e attività influencer.
New-IconCanvas "social" $purple {
    param($g, $p, $b, $a)
    $g.DrawLines($p, @([System.Drawing.Point]::new(33,34),[System.Drawing.Point]::new(93,34),[System.Drawing.Point]::new(93,75),[System.Drawing.Point]::new(63,75),[System.Drawing.Point]::new(47,91),[System.Drawing.Point]::new(49,75),[System.Drawing.Point]::new(33,75),[System.Drawing.Point]::new(33,34)))
    $heart = @([System.Drawing.Point]::new(63,65),[System.Drawing.Point]::new(49,52),[System.Drawing.Point]::new(52,44),[System.Drawing.Point]::new(63,50),[System.Drawing.Point]::new(74,44),[System.Drawing.Point]::new(78,52))
    $g.FillPolygon($b, $heart)
}

# Documento e firma: mercato, proposte e contratti.
New-IconCanvas "mercato" $cyan {
    param($g, $p, $b, $a)
    $g.DrawRectangle($p, 35, 27, 48, 69)
    $g.DrawLine($p, 46, 45, 71, 45); $g.DrawLine($p, 46, 59, 71, 59)
    $g.DrawLine($p, 54, 82, 91, 48)
    $g.DrawLine($p, 83, 47, 92, 56)
}

# Pin e strada: trasferte e spostamenti.
New-IconCanvas "trasferimento" $blue {
    param($g, $p, $b, $a)
    $g.DrawEllipse($p, 48, 25, 32, 32)
    $g.DrawLines($p, @([System.Drawing.Point]::new(50,52),[System.Drawing.Point]::new(64,79),[System.Drawing.Point]::new(78,52)))
    $g.FillEllipse($b, 59, 36, 10, 10)
    $g.DrawLine($p, 31, 94, 97, 94)
}

# Calendario: giornata libera o appuntamento futuro.
New-IconCanvas "calendario" $amber {
    param($g, $p, $b, $a)
    $g.DrawRectangle($p, 31, 34, 66, 60)
    $g.DrawLine($p, 31, 52, 97, 52)
    $g.DrawLine($p, 47, 27, 47, 42); $g.DrawLine($p, 81, 27, 81, 42)
    $g.FillEllipse($b, 45, 64, 8, 8); $g.FillEllipse($b, 60, 64, 8, 8); $g.FillEllipse($b, 75, 64, 8, 8)
}

Write-Host ("Create {0} icone in {1}" -f (Get-ChildItem -LiteralPath $resolvedOutput -Filter 'oggi-*.png').Count, $resolvedOutput)
