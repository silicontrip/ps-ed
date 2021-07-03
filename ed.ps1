param (
	[Parameter(position=0)][string]$Path,
	[Parameter()][string]$Prompt=" "
)

function get-SingleLinenumber
{
	$addr = $args[0]
	$ar = "(\.|$|\d+|\+\d+|\-\d+|\+|\-|/.+/|\?.+\?|'[a-z])"

	if ($addr -match $mmat)
	{
		$mad = $matches[1]

		if ($mad -match "^\d+$")      { return ($mad) }
		if ($mad -match "^\.$" )      { return ($script:currentLine) }
		if ($mad -match '^\$$' )      { return ($script:local.length) }
		if ($mad -match "^\+(\d+)$" ) { return ($script:currentLine + $matches[1]) }
		if ($mad -match "^\-(\d+)$" ) { return ($script:currentLine - $matches[1]) }
		if ($mad -match "^\+$" )      { return ($script:currentLine + 1 ) }
		if ($mad -match "^\-$" )      { return ($script:currentLine - 1 )  }
		if ($mad -match "^'([a-z])$") { return ($script:markedline[$matches[1]]) }
	}

}
function get-MultiLinenumber
{
	$addr = $args[0]

	#write-host "matching -> $addr ($($addr.length))"

	if ($addr.length -eq 0) { return @($script:currentLine,$script:currentLine)}
	if ($addr -match "^,$") { return @(0,$script:local.length) }
	if ($addr -match "^;$") { return @($script:currentLine,$script:local.length) }

	# write-host $addr

	$ar = "\.|$|\d+|\+\d+|\-\d+|\+|\-|/.+/|\?.+\?|'[a-z]"
	$mmat = "($ar),?($ar)?"

	#write-host 

	if ($addr -match $mmat)
	{

		[array]$range = @()

		foreach ($mm in $matches)
		{
			$addr = $mm[0]

			if ($addr -match "^\d+$") { $range = $range + @([int]$addr) }
			if ($addr -match "^\.$" ) { $range = $range + @([int]$script:currentLine) }
			if ($addr -match '^\$$' ) { $range = $range + @([int]$script:local.length) }
			if ($addr -match "^\+(\d+)$" ) { $range = $range + @( [int]$script:currentLine + $matches[1]) }
			if ($addr -match "^\-(\d+)$" ) { $range = $range + @( [int]$script:currentLine - $matches[1]) }
 			if ($addr -match "^\+$" ) { $range = $range + @( [int]$script:currentLine + 1 ) }
			if ($addr -match "^\-$" ) { $range = $range + @( [int]$script:currentLine - 1 )  }
			if ($addr -match "^'([a-z])$" ) { $range = $range + @( [int]$script:markedline[$matches[1]]) }

		}
		return $range
	}
    write-host "?"
    $script:exitError = "unknown range"
}

function ed_add {
	
	$address=$args[0][1]

	if ($address.length -eq 0)
	{
		$ln = $script:currentLine + 1
	} else {
		$ln = get-SingleLinenumber $address
	}

	$script:currentLine = [int]$ln 
    if ($script:local.count -eq 0) {
        write-line "new file"
        $script:currentLine = 0
        }


	$script:inputMode=$true
}

function ed_change {

	$range = get-Multilinenumber $args[0][1]

	$start = $range[0]
	$end = $range[1] 

	if ($start -eq "") { $start = $currentLine }
	if ($end -eq "") { $end = $currentLine }
	if ($end -lt $start) {
        write-host "?"
        $script:exitError = "invalid range" 
        return 
    }

	# off by one?
	$script:cutBuffer=$script:local[$start,$end]
	$script:local = @($script:local[0..$start]) + @($script:local[$end..$script:ll])

	$script:currentLine = [int]$start
	$script:inputMode = $true

}

function ed_delete {

	$range = get-Multilinenumber $args[0][1]

	$start = $range[0]
	$end = $range[1] 

    if ($end.length -eq 0) {
        $end = $start
    }

	if ($start -eq "")  { $start = $currentLine }
	if ($end -eq "")  { $end = $currentLine }

write-host "start: $start end: $end"

	if ($end -lt $start) { 
        write-host "?"
        $script:exitError = "invalid range"
        return 
    }

	# off by one?

    $astart = $start -1
    $aend = $end -1

    write-host "start: $astart end: $aend"
    write-host ">$($script:local[$astart])<"

	$script:cutBuffer=$script:local[$astart,$aend]

    write-host "CUT>$($script:cutBuffer)<"
	$script:local = @($script:local[0..$start]) + @($script:local[$end..$script:ll])

	$script:currentLine = [int]$start
	#$script:inputMode = $true
}

function ed_edit {

	$script:filename = $args[0][1]

	if ( -not $script:changes) {
		$script:local = get-content $script:filename
		$script:currentLine = $local.length
		$script:localLength = ($local -join '').length
		write-host $script:localLength
	}
}

function ed_unconditionaledit {

	$script:filename = $args[0][1]

		$script:local = get-content $script:filename
		$script:currentLine = $local.length
		$script:localLength = ($local -join '').length
		write-host $script:localLength
}

function ed_filename {

	if ($args[0][1].length -gt 0)
	{
		$script:filename = $args[0][1]
	} else {
		write-host $script:filename
	}

}

function ed_helperror {
	write-host $script:exiterror
}

function ed_mark {
	$address=$args[0][1]
	$markName = $args[0][3]

	if ($address.length -eq 0)
	{
		$ln = $script:currentLine
	} else {
		$ln = get-SingleLinenumber $address
	}

	$script:markedline[$markName]=$ln
}

function ed_print {

	$range = get-multilinenumber $args[0][1]

	$start = $range[0]
	$end = $range[1]

	foreach ($line in $local[$start..$end])
	{
		write-host ">$line<"
	}
}

function ed_prompt {
	if ($prompt -eq " ")
	{
		$prompt="*"
	} else {
		$prompt=" "
	}
}

function ed_quit {
	if (-not $script:changes) 
	{
		exit
	}
}

function ed_unconditionalQuit {
	exit
}

$addressRegex = "\.|$|\d*|\+\d+|\-\d+|\+|\-|/.+/|\?.+\?|'[a-z]"

$commands=@(
	@{ match="^(')([a-z])$"; cmd="ed_gotomark"},
	@{ match="^($addressRegex)(a)$"; cmd="ed_add"},
	@{ match="^($addressRegex,?$addressRegex?)(c)$";  cmd="ed_change" },
	@{ match="^($addressRegex,?$addressRegex?)(d)$"; cmd="ed_delete" },
	@{ match="^e (\S+)$"; cmd = "ed_edit"},
	@{ match="^E (\S+)$"; cmd = "ed_unconditionaledit"},
	@{ match="^f *(\S+)?$"; cmd = "ed_filename"},
	@{ 
		match="^h$"; 
	cmd = "ed_helperror"
	},
	@{ match="^($addressRegex)(k)([a-z])$"; cmd="ed_mark"},
	@{ match="^(($addressRegex)?,?($addressRegex)?)(p)$"; cmd="ed_print" },
	@{ match="^P$"; cmd = "ed_prompt" },
	@{ match="^q$"; cmd = "ed_quit"},
	@{ match="^Q$"; cmd = "ed_unconditionalquit"}
)


$changes=$false
$inputMode=$false 
$currentLine=0
$markedLines=@{}
$filename = $path
$cutBuffer = ""
$filename = ""


if ($path) {
	$local = get-content $path
	$currentLine = $local.length
	$localLength = ($local -join '').length
	$filename = $path
	write-host $localLength
} else {
    $local=@('')
    }

while ($true) {
	$readCmd = read-host -prompt $prompt

	if ($inputmode) {
		#input mode
		if ($readCmd -eq ".") { $inputmode = $false }
		else
		{
			$ll = $local.length
			#write-host "0,$currentLine,$ll"

			$local = @($local[0..$currentLine]) + @($readCmd) + @($local[$currentLine..$ll])

			$ll = $local.length
			$currentLine++
			$changes=$true
		}	
	}
	else  
	{
		foreach ($m in $commands)
		{
			$match = $m['match']
			if ($readCmd -match $match)
			{
				& $m['cmd'] $matches
			}
		}
	}		

}