SET resultsfile=%~1

if '%resultsfile%'=='' SET file=results.xml

DotNetXsl.exe "%resultsfile%" NUnitExpandableTestSuites.xslt "%resultsfile%-Expandable.html"
DotNetXsl.exe "%resultsfile%" NUnitAggregatedErrors.xslt "%resultsfile%-Aggregated.html"