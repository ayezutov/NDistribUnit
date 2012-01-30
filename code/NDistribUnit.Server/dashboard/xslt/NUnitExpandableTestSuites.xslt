<?xml version="1.0" standalone="yes"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns="http://www.w3.org/TR/html4/strict.dtd" exclude-result-prefixes="">
	<xsl:output method="html" encoding="ISO-8859-1" standalone="yes" version="1.0" indent="yes"/>
	<xsl:param name="applicationPath">.</xsl:param>
	<xsl:template match="/">
		<div id="report">
			<script type="text/javascript">
				<xsl:text>
				function toggleDiv(imgId, divId)
				{
					eDiv = document.getElementById(divId);
					eImg = document.getElementById(imgId);

					if ( eDiv.style.display == "none" )
					{
						eDiv.style.display="block";
						eImg.className="button-collapse";
				 	}
					else
					{
						eDiv.style.display = "none";
						eImg.className="button-expand";
					}
				}
				
				
				function toggleTr(imgId, trId)
				{
					eTr = document.getElementById(trId);
					eImg = document.getElementById(imgId);

					if ( eTr.style.display == "none" )
					{
						expandTR(eImg, eTr);
				 	}
					else
					{
						collapseTR(eImg, eTr);
					}
				}
				
				function collapseTR(eImg, eTr)
				{
					eTr.style.display = "none";
					eImg.className="button-expand";
				}
				
				function expandTR(eImg, eTr)
				{
						/* Setting a TR to display:block doesn't work in proper browsers
						but IE6's dodgy CSS implementation doesn't know table-row so
						we need to try...catch it */
						try
						{
							eTr.style.display="table-row";
						}
						catch(e)
						{
							eTr.style.display="block";
						}
						eImg.className="button-collapse";
				}				
				
				var failedTestSuites = [];
				var allTestSuites = [];
				</xsl:text>
				function collapseAll()
				{
					for (i in allTestSuites)
					{
						collapseTR(document.getElementById("img-"+allTestSuites[i]), document.getElementById(allTestSuites[i]))
					}
				}
				
				function expandAll()
				{
					for (i in allTestSuites)
					{
						expandTR(document.getElementById("img-"+allTestSuites[i]), document.getElementById(allTestSuites[i]))
					}
				}
				
				function expandFailed()
				{
					for (i in failedTestSuites)
					{
						expandTR(document.getElementById("img-"+failedTestSuites[i]), document.getElementById(failedTestSuites[i]))
					}
				}
			</script>
			<style type="text/css">
				#master { font: small Verdana }
				#master table { border-collapse:collapse; table-layout: fixed;}
				#master tr { height: 25px; vertical-align: middle; }
				#master th { text-align: center; border-bottom: 1px solid black; border-top: 1px solid black;}
				#master div { box-sizing: content-box; -moz-box-sizing: content-box; }
				#master img { display: inline-block; vertical-align: middle; }

                .button-expand { width: 15px; height: 15px; background-repeat: no-repeat; background-image: url(data:image/gif;base64,R0lGODlhDwAPAPcAAAgICAgIEBAQEBAQGBgYGBgYISEhISEhKSkpMTExMTExOUJCSkpKY1JSUlJSWlJSa1paa1pac2NjY2Njc2Nje2tra3Nzc3NzjISEhIyMjJSUlJycnKWlpbW1tb29vcbGxs7Ozt7e3ufn5+/v7/f39////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////yH5BAEAACUALAAAAAAPAA8AAAiHAEsIFEGChMCDCAWGSIAhxIiECT8AEODgwwiDEEt8ELCAQAMPBDNujPCgQIIOISMKmHBhwgEFHFIeHCniAoUDBjSEwChwo4ifFyBQ/CACIU2bBhBYQGl0JVIEFTJYbBqBQlIJGEDy1EjxAAIJGTzsjBhAwNcMKLf2BJAgqli1CjNo0KAVYkAAADs=);
}
                .button-collapse { width: 15px; height: 15px; background-repeat: no-repeat; background-image: url(data:image/gif;base64,R0lGODlhDwAPAPcAAAAAAAAACBAQEBAQGBgYGBgYISEhITExMTExOTExQjk5OUJCUkpKUlJSUlJSa1paWmNjY2Njc2tre2trhHNzc3NzjHt7e4SEhKWlpa2trbW1tb29vcbGxs7OztbW1t7e3ufn5+/v7/f39////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////yH5BAEAACMALAAAAAAPAA8AAAiIAEcIHEiwoMGDA0OAWMiwIYgQIj4oMECxokUDED6AUCAgAYOPC0AaCPBAg4gNCAxEqMCwwoIBDyyYDJEBQQEHFSpMSEDgAQUMHUaIAKGhAQEHEhAQgPCzQwiBQzcYJYDRQgYPTwcO5QCBAYULGT5kJSgiRIcMGDZ8EIGwLEO2CKHCjUu3LsGAAAA7);
}

                .img-success { width: 16px;height: 16px;background-repeat: no-repeat; display:inline; background-image: url(data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAACxIAAAsSAdLdfvwAAAAadEVYdFNvZnR3YXJlAFBhaW50Lk5FVCB2My41LjEwMPRyoQAAAipJREFUOE+Vks1rE0EYxvdP0EJFim1Fwa3VxOzOZpOmIlsQlKattODBil5Fdrd4UNSLzUGsrVA10dTNB0VQsQcL6inRXlRE04MkShS14EcXkeIHVLQxJo8zm3ZNTFvoLM/Ozsvym2eeeTkA3Er1/tsbDD8awLQ5DW7seRStcQnbYzJaY14qT5kq1764DCb5CoHPL8MwDHDXM1FI5x2oVxrB+/lltfXIZpAIgRgU4N/TTo2DowAD7kEeqqoil8strrk5jGduQb5M0Ni7HkLIic7ujn8AmQL0vj5WqBqFQgFPPjxECz1a3c618OzzQLwoVAKYgxKgWHqKRQtUoPPrmVdou6qAP7wBLV4v7mbvUIBIAZ2VR9B0HTOzJtLmJNiubHz9+QXdY10QzjlR27AG2ZdZJN4mqgHyWR4dB3ejbXQH5Jgb4duXkM//xsn7R0EMCXXeGoRCQctZcioBYjkoy8A9REPUVQw9OAPJELHtVDMGbp6m3xKatU1QFMUKl43kVHIRAMtA1/Dj13e0X9sFMkIgBF2Qwi6srl+FdDpth7s0YP4WRp9FIVIXxCDY2NsATdXsTJZ3MA/4PPsJvpgPW040oXZdDUzTtG9lAVC6hfIMBpvsPsj/ySM+EYbD5UQqlarYfQlAFO5hBw7178fTjxOWHr9L4N6LcXu9UGfzyOQFKx/bwY1MBGJEoBL/E+35qhr7R6AA2ok9XaVGYq9jgePoObB3ReoPBCzAX052pd0ZB2HYAAAAAElFTkSuQmCC);
}
                .img-warning { width: 16px; height: 16px; background-repeat: no-repeat;  display:inline; background-image: url(data:image/gif;base64,R0lGODlhEAAQAOb/AEk7PcDAwEk7TlVPV2Zfa1FNVGZfbVFNVmReblxWb19acJSRnl5ZcV5ZcF5acSIUcg8Aaj8vq05JbUVDZQQAVQMAOjEwYUhHcgAAPQAAEwAACAAAB3N0tUZNlv/8AP//AOzpAv//A//9Bf//Bf//Bt7bBv//Cf/+EP/+Ev//EvXzFP//Hf//LWFhRpeXj//4AP/5AP/6AOrjALKvAf/5AvryAv/3A//7BurkB//2DP/wAP/0AP/1APLlAP/0Cv/0Db63E//qAP/sAP/tAMS3APvrBP/rBf/tDt7ODqmgGf/kAP/nAP/oAP/pAOnSAOfTAOrSAv/nA//kB/jjCv/lDP/eAP/fAP/gAP/fAezOA+jMBfPWBvzfDtW9DoJ3JP/ZANO3EMKqHP/TAP/VAP/XAPfLAOjDAOzGAs6uEu7QNf/OAP/RAP7MAPjGAO6/AOvGHq2SG+/KMvzHAPnGAPjFAPfEAEU9H1pWTCAZEWNTRFpPTAAAAAAAAAAAAAAAAAAAACH5BAEAAAEALAAAAAAQABAAQAe1gAGCg4SFgz1QPh9eD143I10chgE9NiUzJjiShD1nVGRVTBsYYTovNCJom5wqLCCrhj1FJBkUAiFIsJRZR1ZBW3AxOz8kYJtxbnVtc2tkbGJybXRqYxaTPR8uk4UyJisfLduCPSYaFXofd9tOOR92EHkpHwWGWlNGHkkAQDUnHgeEzHCR8kXIEyJDeMBAAWOAoDdlsHy50gRPhDRMviiJsmSCoAUSEjBwgICAAgYMGhi40CFAIAA7);
}
           
                .img-failure { width: 16px;height: 16px;background-repeat: no-repeat; display:inline; background-image: url(data:image/gif;base64,R0lGODlhEAAQAMQAAAAAANMjI8wAAOV7e9pEROucnOBlZdxPT9c5OemSku2np9EYGNYyMuJwcPCyss8SEt5aWueGhv///wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACH5BAUUABIALAAAAAAQABAAAAVaoCSOZGkCaGqOgKMUSTQAa/uiQ2PQpA2jOsiBJ/HFUAYhAcEDvGIzgBLBaMJkDdRhyQg0oToUN7BoYpNbKvnRzKHHi0eV9Uat2SVAGhUXEHtqfX95KSgrhyYhADs=);
}

				.col0 { width: 20px;min-width: 20px; max-width: 20px; float:left;}
				.col1 { overflow: hidden;}
				.col2 { width: 310px; min-width: 310px; max-width: 310px;float:left;}
				.col3 { width: 50px; text-align: center; min-width: 40px; max-width: 40px; float:left;overflow: hidden;overflow: hidden;}

				th.col2 { -moz-box-sizing: content-box;}
				.col2 div { margin: 0px; padding: 0px; display:block; float:left; height: 15px; }

				.testAssembly { border: 1px dotted gray; padding: 5px 2px 5px 2px; margin-bottom: 10px; min-width: 400px}
				.testAssembly table { width: 100%; min-width: 400px; }

				.testAssembly .testTable { margin: 0 auto; left: 25px; width: 95%; border: 1px solid gray;}
				.testTable td { border: 1px solid gray; overflow: hidden; padding: 2px 5px 2px 5px; background-color: inherit;}
				.testName { white-space: nowrap;}
				.testMsg { width: 100%; }
				.testMsg div { overflow: auto; width: 100%;}

				tr.succeeded { background-color: rgb(145,222,121);}
				tr.failed { background-color: rgb(245,147,147);}
				tr.ignored { background-color: rgb(252,242,80);}
				tr.unknown { background-color: gray;}

				div.succeeded { border: 0px solid green; background-color: rgb(59,216,23);}
				div.failed { border: 0px solid rgb(177,26,26); background-color: rgb(228,95,95);}
				div.ignored { border: 0px solid rgb(233,221,26); background-color: yellow;}
				div.unknown { border: 0px solid black; background-color: gray;}
				
				TD.suiteFailed {background-color: Red; color: white; font-weight: bold;}
				.suiteFailed PRE {background-color: Red;}
				
				TD.machineNames { background-color: Silver; font-family:Arial; font-size:10px;}
				DIV.machineNames { font-family:Arial; font-size:10px; color: Gray;}

				.category { padding-left: 0px; font-style: oblique; font-size: 10px;}
				.clickable { cursor: pointer; }
				
				.summaryTable { border: solid 1px black; }
				.summaryTable TD { border: solid 1px black; height: 50px;}
				.summaryTable .counts { width: 50px; }
				.summaryTable .message { }
				.summaryTable .message DIV { width: 700px; overflow: auto; }
				.summaryTable .testNames DIV{ font-size: 9px; color: Black; }
				.summaryTable TR.critical {background-color: #FF5555}
				.summaryTable TR.major {background-color: #FFAAAA;}
				.summaryTable TR.medium {background-color: #FFFF99;}
				.summaryTable TR.minor {background-color: #FFFFDD;}
				
			</style>
			<div id="summary">
				<h3>Summary</h3>
				<table>
					<tbody>
						<tr>
							<td>Assemblies tested:</td>
							<td>
								<xsl:value-of select="count(//test-results)"/>
							</td>
						</tr>
						<tr>
							<td>Tests executed:</td>
							<td>
								<xsl:value-of select="count(//test-results//test-case[@executed = 'True'])"/>
							</td>
						</tr>
						<tr>
							<td>Passes:</td>
							<td>
								<xsl:value-of select="count(//test-results//test-case[@executed = 'True' and @success = 'True'])"/>
							</td>
						</tr>
						<tr>
							<td>Fails:</td>
							<td>
								<xsl:value-of select="count(//test-results//test-case[@executed = 'True' and @success = 'False'])"/>
							</td>
						</tr>
						<tr>
							<td>Ignored:</td>
							<td>
								<xsl:value-of select="count(//test-results//test-case[@executed = 'False'])"/>
							</td>
						</tr>
					</tbody>
				</table>
			</div>
			
			<a href="#" onclick="expandAll();return false;">Expand All</a>
			<a href="#" onclick="collapseAll();return false;">Collapse All</a>
			<a href="#" onclick="expandFailed();return false;">Expand Failed</a>
			<xsl:for-each select="//test-results[test-suite]">
				<xsl:variable name="divId">
					<xsl:value-of select="generate-id(test-suite/@name)"/>
				</xsl:variable>
				<div style="margin-bottom: 5px;">
					<xsl:attribute name="onclick"><xsl:text>toggleDiv('img-</xsl:text><xsl:value-of select="$divId"/><xsl:text>','</xsl:text><xsl:value-of select="$divId"/><xsl:text>')</xsl:text></xsl:attribute>
					<xsl:attribute name="class"><xsl:text>clickable</xsl:text></xsl:attribute>
					<div title="Toggle display of Tests contained within this assembly" class="button-collapse">
						<xsl:attribute name="id"><xsl:text>img-</xsl:text><xsl:value-of select="$divId"/></xsl:attribute>
					</div>
					<xsl:text>&#0160;</xsl:text>
					<strong>
						<xsl:call-template name="getSuiteName">
							<xsl:with-param name="name" select="test-suite[test-suite]/@name"/>
						</xsl:call-template>
					</strong>
					<!--
					<xsl:text>&#0160;-&#0160;</xsl:text>
					<xsl:value-of select="@total + @not-run"/>
					<xsl:text>&#0160;tests&#0160;(</xsl:text>
					<xsl:value-of select="@total - @failures" />
					<xsl:text>&#0160;passed,&#0160;</xsl:text>
					<xsl:value-of select="@failures" />
					<xsl:text>&#0160;failed,&#0160;</xsl:text>
					<xsl:value-of select="@not-run" />
					<xsl:text>&#0160;didn't run)</xsl:text> -->
				</div>
				<div class="testAssembly">
					<xsl:attribute name="id"><xsl:value-of select="$divId"/></xsl:attribute>
					<table>
						<tr>
							<th class="col1">
								<div class="col0">&#0160;</div>
								<div class="col3">Passes</div>
								<div class="col2">Results</div>
							Test Fixture</th>
						</tr>
						<xsl:for-each select=".//test-suite[results/test-case]">
							<xsl:sort select="@name" order="ascending" data-type="text"/>
							<xsl:variable name="testsId">
								<xsl:value-of select="generate-id(results/test-case/@name)"/>
							</xsl:variable>
							<tr>
								<xsl:attribute name="onclick"><xsl:text>toggleTr('img-</xsl:text><xsl:value-of select="$testsId"/><xsl:text>','</xsl:text><xsl:value-of select="$testsId"/><xsl:text>')</xsl:text></xsl:attribute>
								<xsl:attribute name="class"><xsl:text>clickable</xsl:text></xsl:attribute>
								<td class="col1">
									<div class="col0">
										<div title="Toggle display of the tests within this text fixture" class="button-expand">
											<xsl:attribute name="id"><xsl:text>img-</xsl:text><xsl:value-of select="$testsId"/></xsl:attribute>
										</div>
									</div>
									<div class="col3">
										<xsl:call-template name="GetTests">
											<xsl:with-param name="CurrentSuite" select="."/>
										</xsl:call-template>
									</div>
									<div class="col2">
										<xsl:apply-templates select="." mode="graph"/>
									</div>
									<span>
										<xsl:value-of select="@name"/>
									</span>
								</td>
							</tr>
							<tr>
								<xsl:attribute name="id"><xsl:value-of select="$testsId"/></xsl:attribute>
								<xsl:attribute name="style"><xsl:text>display:none;</xsl:text></xsl:attribute>
								<td>
									<xsl:if test="count(results/test-case[@success='False'])>0">
										<script>failedTestSuites.push('<xsl:value-of select="$testsId"/>')</script>
									</xsl:if>
									<script>allTestSuites.push('<xsl:value-of select="$testsId"/>')</script>
									<xsl:apply-templates select="." mode="tests"/>
								</td>
							</tr>
						</xsl:for-each>
					</table>
				</div>
			</xsl:for-each>
		</div>
	</xsl:template>
	<!-- Template for a particular TestFixture -->
	<xsl:template match="test-suite[results/test-case]" mode="tests">
		<table class="testTable" style="float:left;">
			<xsl:if test="count(./properties/property[@name='ndistribunit.agent-name'])>0">
				<tr>
					<td colspan="2" class="machineNames">
						<xsl:for-each select="./properties/property[@name='ndistribunit.agent-name']">
							<xsl:if test="position()>1">
								<xsl:text>,&#0160;</xsl:text>
							</xsl:if>
							<xsl:value-of select="@value"/>
						</xsl:for-each>
					</td>
				</tr>
			</xsl:if>
			<xsl:if test="count(./reason/message)>0">
				<tr>
					<td colspan="2">
						<pre>
							<xsl:value-of select="./reason/message"/>
						</pre>
					</td>
				</tr>
			</xsl:if>
			<xsl:if test="StackTrace or failure">
				<tr>
					<td colspan="2" class="suiteFailed">
						<pre>
							<xsl:if test="StackTrace">
								<xsl:value-of select="./StackTrace/text()"/>
							</xsl:if>
							<xsl:if test="failure">
								<xsl:value-of select="./failure/message"/>
								<br/>
								<xsl:value-of select="./failure/stack-trace"/>
							</xsl:if>
						</pre>
					</td>
				</tr>
			</xsl:if>
			<xsl:for-each select="results/test-case[@success='False']">
				<xsl:sort select="@name" order="ascending" data-type="text"/>
				<xsl:apply-templates select="." mode="test"/>
			</xsl:for-each>
			<xsl:for-each select="results/test-case[@executed='False']">
				<xsl:sort select="@name" order="ascending" data-type="text"/>
				<xsl:apply-templates select="." mode="test"/>
			</xsl:for-each>
			<xsl:for-each select="results/test-case[@success='True']">
				<xsl:sort select="@name" order="ascending" data-type="text"/>
				<xsl:apply-templates select="." mode="test"/>
			</xsl:for-each>
		</table>
	</xsl:template>
	<!-- Writes a line in the table for a particular test -->
	<xsl:template match="results/test-case" mode="test">
		<tr>
			<xsl:attribute name="class"><xsl:call-template name="GetClass"><xsl:with-param name="Executed" select="@executed"/><xsl:with-param name="Succeeded" select="@success"/></xsl:call-template></xsl:attribute>
			<td class="testName">
				<xsl:if test="categories">
					<xsl:for-each select="categories/category">
						<div class="category">
							<xsl:text>[</xsl:text>
							<xsl:value-of select="@name"/>
							<xsl:text>]</xsl:text>
						</div>
					</xsl:for-each>
				</xsl:if>
				<xsl:call-template name="GetImage">
					<xsl:with-param name="Executed" select="@executed"/>
					<xsl:with-param name="Succeeded" select="@success"/>
				</xsl:call-template>
				<xsl:text>&#0160;</xsl:text>
				<xsl:call-template name="GetTestName">
					<xsl:with-param name="Name" select="@name"/>
				</xsl:call-template>
				<div class="machineNames"><xsl:value-of select="@name"></xsl:value-of></div>
				<xsl:if test="properties/property[@name='ndistribunit.agent-name']">
					<div class="machineNames">
					    <xsl:text disable-output-escaping="yes"><![CDATA[<b>Machines: </b>]]></xsl:text>
						<xsl:for-each select="properties/property[@name='ndistribunit.agent-name']">
							<xsl:if test="position()>1">
								<xsl:text>,&#0160;</xsl:text>
							</xsl:if>
							<xsl:value-of select="@value"/>
						</xsl:for-each>
					</div>
				</xsl:if>
			</td>
			<td class="testMsg">
				<pre>
					<xsl:value-of select=".//message"/>
					<xsl:if test=".//stack-trace">
						<br/>
						<div style="margin-top: 5px;">
							<xsl:value-of select=".//stack-trace"/>
						</div>
					</xsl:if>
				</pre>
			</td>
		</tr>
	</xsl:template>
	<!-- Gets the class to use depending upon whether the test passed or failed -->
	<xsl:template name="GetClass">
		<xsl:param name="Executed"/>
		<xsl:param name="Succeeded"/>
		<xsl:choose>
			<xsl:when test="$Executed='False'">
				<xsl:text>ignored</xsl:text>
			</xsl:when>
			<xsl:when test="$Executed='True' and $Succeeded='False'">
				<xsl:text>failed</xsl:text>
			</xsl:when>
			<xsl:when test="$Executed='True' and $Succeeded='True'">
				<xsl:text>succeeded</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>unknown</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	<!-- Gets the image tag to show success or failure -->
	<xsl:template name="GetImage">
		<xsl:param name="Executed"/>
		<xsl:param name="Succeeded"/>
		<xsl:choose>
			<xsl:when test="$Executed='False'">
			    <div class="img-warning" title="The test wasn't run"></div>
			</xsl:when>
			<xsl:when test="$Executed='True' and $Succeeded='False'">
				<div class="img-failure" title="The test failed"></div>
			</xsl:when>
			<xsl:when test="$Executed='True' and $Succeeded='True'">
			    <div class="img-success" title="The test succeeded"></div>
			</xsl:when>
		</xsl:choose>
	</xsl:template>
	<!-- Template to draw a graph for a particular TestFixture -->
	<xsl:template match="test-suite[results/test-case]" mode="graph">
		<xsl:variable name="numTests">
			<xsl:call-template name="GetNumberOfTests">
				<xsl:with-param name="CurrentSuite" select="."/>
			</xsl:call-template>
		</xsl:variable>
		<xsl:variable name="chartWidth" select="300"/>
		<xsl:for-each select="results/test-case">
			<xsl:sort select="@executed" order="descending" data-type="text"/>
			<xsl:sort select="@success" order="descending" data-type="text"/>
			<xsl:element name="div">
				<xsl:attribute name="style"><xsl:text>width:</xsl:text><!--<xsl:value-of select="(350 - ($numTests * 2)) div $numTests"/>--><xsl:value-of select="ceiling(($chartWidth*position()) div $numTests) - ceiling(($chartWidth*(position()-1)) div $numTests)"/><xsl:text>px;</xsl:text></xsl:attribute>
				<xsl:attribute name="class"><xsl:call-template name="GetClass"><xsl:with-param name="Executed" select="@executed"/><xsl:with-param name="Succeeded" select="@success"/></xsl:call-template></xsl:attribute>
				<xsl:text>&#160;</xsl:text>
			</xsl:element>
		</xsl:for-each>
		<br/>
		<xsl:element name="div">
			<xsl:attribute name="style"><xsl:text>width:</xsl:text><xsl:value-of select="$chartWidth+5"/><xsl:text>px; height:1px;</xsl:text></xsl:attribute>
			<xsl:text>&#160;</xsl:text>
		</xsl:element>
	</xsl:template>
	<!-- Calculates the total number of tests in a particular testfixture -->
	<xsl:template name="GetNumberOfTests">
		<xsl:param name="CurrentSuite"/>
		<xsl:value-of select="count($CurrentSuite/results/test-case)"/>
	</xsl:template>
	<!-- Gets the text for the third column -->
	<xsl:template name="GetTests">
		<xsl:param name="CurrentSuite"/>
		<xsl:text>(</xsl:text>
		<xsl:value-of select="count($CurrentSuite/results/test-case[@success='True'])"/>
		<xsl:text>/</xsl:text>
		<xsl:value-of select="count($CurrentSuite/results/test-case)"/>
		<xsl:text>)</xsl:text>
	</xsl:template>
	<!-- Takes a full path and returns the filename -->
	<xsl:template name="getSuiteName">
		<xsl:param name="name"/>
		<xsl:choose>
			<xsl:when test="contains($name, '\')">
				<xsl:call-template name="getSuiteName">
					<xsl:with-param name="name" select="substring-after($name, '\')"/>
				</xsl:call-template>
			</xsl:when>
			<xsl:when test="contains($name, '/')">
				<xsl:call-template name="getSuiteName">
					<xsl:with-param name="name" select="substring-after($name, '/')"/>
				</xsl:call-template>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="$name"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	<!-- Takes a fully qualified test name and returns just the name of the test -->
	<xsl:template name="GetTestName">
		<xsl:param name="Name"/>
		<xsl:choose>
			<xsl:when test="contains($Name, '.')">
				<xsl:call-template name="GetTestName">
					<xsl:with-param name="Name" select="substring-after($Name, '.')"/>
				</xsl:call-template>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="$Name"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
</xsl:stylesheet>
