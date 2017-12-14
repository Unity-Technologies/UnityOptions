use Cwd;
use File::Copy;
use File::Path;
use File::Basename;

my $currentdir              = getcwd();
my $scrachDirRelative       = ".format-scratch";
my $scratchDir              = "$currentdir/$scrachDirRelative";
my $formatFilesListPath     = "$scratchDir/format-files.rsp";
my $stagedFileListPath      = "$scratchDir/format-staged-files.txt";
my $checkStderrFilePath     = "$scratchDir/format-check-stderr.log";
my $checkStdoutFilePath     = "$scratchDir/format-check-stdout.log";
my $stagedBackupDirRelative = "$scrachDirRelative/backup";
my $stagedBackupDir         = "$currentdir/$stagedBackupDirRelative";
my $exitCode                = 0;
my $cleanupTmpFiles         = 0;
my $debug                   = 0;

# With this option on, when staged files are formatted, the working directory version will also be formatted
# This avoids a potentially odd situation where you have working directory modifications after formatting the staged
# files because the work dir version still has the old, unformatted change
my $formatWorkFilesWithStaged = 1;

my $isCheckRun  = $#ARGV >= 1 && $ARGV[1] eq "check";
my $isPreCommit = $#ARGV >= 0 && $ARGV[0] eq "pre-commit";
my $isWorkingTree = $#ARGV == -1;
my $isStaged      = $#ARGV >= 0 && $ARGV[0] eq "staged";
my $isAll         = $#ARGV >= 0 && $ARGV[0] eq "all";

# Map the pre-commit mode to w/e behavior we want during pre-commit.  By having a dedicated flag for pre-commit we have flexibility
# to change what we do during pre-commit without having to require everyone update their pre-commit file
if ($isPreCommit)
{
    $isCheckRun = 1;
    $isStaged   = 1;
}

my $restoreStagingMagic = 0;
my $nothingToFormat     = 0;

# If we are doing a check only, then we never want to check working dir files since they are not the files
# that are about to be committe
if ($isCheckRun)
{
    $includeAllWorkFilesWithStaged = 0;
    $formatWorkFilesWithStaged     = 0;
}

if (!-d "$scratchDir")
{
    mkdir("$scratchDir");
}

if ($isWorkingTree)
{
    # Format only changed files
    system("git diff --name-only --diff-filter=ACMR > $formatFilesListPath") eq 0 or die("git command failed\n");
}

if ($isStaged)
{
    system("git diff --name-only --cached --diff-filter=ACMR > $formatFilesListPath") eq 0 or die("git command failed\n");

    copy("$formatFilesListPath", "$stagedFileListPath") or die "Copy failed: $!";

    if (!-z "$formatFilesListPath")
    {
        $restoreStagingMagic = 1;
        if (-d "$stagedBackupDir")
        {
            rmtree("$stagedBackupDir");
        }

        mkdir("$stagedBackupDir");

        open(my $fh, '<:encoding(UTF-8)', $stagedFileListPath) or die "Could not open file '$stagedFileListPath' $!";

        while (my $stagedFile = <$fh>)
        {
            # First back up the version in the working directory in case there are local modifications
            chomp $stagedFile;
            my $src     = "$currentdir/$stagedFile";
            my $dest    = "$stagedBackupDir/$stagedFile";
            my $destDir = dirname("$dest");
            mkpath($destDir);
            copy("$src", "$dest") or die "Copy failed: $!";

            # This will get the working tree in the state of the staged change, overwriting any local changes already in the working dir (we backed these up)
            system("git checkout $stagedFile");

            if (!($isCheckRun))
            {
                if ($formatWorkFilesWithStaged)
                {
                    system("echo $stagedBackupDirRelative/$stagedFile>> $formatFilesListPath")
                }
            }
        }

        if (!($isCheckRun))
        {
            system("git reset");
        }

        close($fh);
    }
}

if ($isAll)
{
    system("git ls-files > $formatFilesListPath") eq 0 or die("git command failed\n");
}

if (-z "$formatFilesListPath")
{
    $nothingToFormat = 1;
    printf "No files found to format\n" if (!$isCheckRun);
}
else
{
    my $homeDir = $ENV{"HOME"};

    if ($^O eq "MSWin32")
    {
        $homeDir = $ENV{"USERPROFILE"};
    }

    if ($isCheckRun)
    {
        # Check only
        system("perl $homeDir/unity-meta/Tools/Format/format.pl --dry-run --hgroot $currentdir \@$formatFilesListPath 2> $checkStderrFilePath > $checkStdoutFilePath") eq 0 or die("one or more files require formatting\n");
        if (!(-z "$checkStderrFilePath"))
        {
            $exitCode = 1;
            PrintFile($checkStderrFilePath);
        }

        PrintFile($checkStdoutFilePath);
    }
    else
    {
        system("perl $homeDir/unity-meta/Tools/Format/format.pl --showfiles --hgroot $currentdir \@$formatFilesListPath") eq 0 or die("formatting tool failed\n");
    }
}

if ($restoreStagingMagic)
{
    open(my $fh, '<:encoding(UTF-8)', $stagedFileListPath) or die "Could not open file '$stagedFileListPath' $!";

    while (my $stagedFile = <$fh>)
    {
        # only add back the file when doing a non-check run
        if (!($isCheckRun))
        {
            system("git add $stagedFile");
        }

        # Now move the backed up files back into their normal location
        chomp $stagedFile;
        my $dest = "$currentdir/$stagedFile";
        my $src  = "$stagedBackupDir/$stagedFile";

        # If we are going to clean up, then it's safe to move
        # otherwise err on the side of caution and make a copy so that the original backups are still around
        if ($cleanupTmpFiles)
        {
            move("$src", "$dest") or die "Move failed: $!";
        }
        else
        {
            copy("$src", "$dest") or die "Copy failed: $!";
        }
    }

    close($fh);
}

if ($cleanupTmpFiles || $nothingToFormat)
{
    rmtree("$scratchDir");
}
else
{
    printf "\nBackup and log files available in : $scratchDir\n" if (!$isCheckRun);
}

if ($exitCode && $isPreCommit)
{
    printf "\n";
    printf "Commit blocked.  One or more staged files require formatting\n";
    printf "\n";
    printf "To format staged files run:\n";
    printf "    format staged\n";
    printf "\n";
    printf "To format working tree files run:\n";
    printf "    format\n";
}

exit $exitCode;

sub PrintFile
{
    my $filename = shift(@_);
    open(my $fh, '<:encoding(UTF-8)', $filename) or die "Could not open file '$filename' $!";

    while (my $row = <$fh>)
    {
        chomp $row;
        print "$row\n";
    }

    close($fh);
}
