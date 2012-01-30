<?xml version="1.0" standalone="yes"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns="http://www.w3.org/TR/html4/strict.dtd" xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="" xmlns:user="epam.com/userDefinedFunctions">
	<msxsl:script language='CSharp' implements-prefix='user'>
	<![CDATA[
		public string GetMessage(XPathNavigator current)
		{
			string result = string.Empty;
			System.Text.RegularExpressions.Regex urlRegex = new System.Text.RegularExpressions.Regex(
				@"((https?|ftp|gopher|telnet|file|notes|ms-help):((//)|(\\\\))[\w\d:#%/;$()~_?\-=\\\.&]*)");

			XPathNavigator stackTraceNode = current.SelectSingleNode("../stack-trace");
			string stackTrace = stackTraceNode != null ? stackTraceNode.Value : string.Empty;

			if (current.Value != null && current.Value.Contains("TestFixtureSetUp failed"))
			{
				XPathNavigator testSuiteMessageNode = current.SelectSingleNode("../../../../failure/message");
				XPathNavigator testSuiteStackTraceNode = current.SelectSingleNode("../../../../failure/stack-trace");

				result = testSuiteMessageNode != null ? testSuiteMessageNode.Value : current.Value;
				stackTrace = testSuiteStackTraceNode != null ? testSuiteStackTraceNode.Value : stackTrace;
			}
			else
				result = current.Value;

			if (stackTrace.Contains("at CommonTests.XmlCompare.XmlAssert.AssertAreEqual"))
				return "XML comparison failed";
				
			if (current.Value != null && current.Value.StartsWith("The download failed, when trying to download from "))
				return "The download failed, when trying to download a file";

			return result != null
			       	? urlRegex.Replace(result, delegate(System.Text.RegularExpressions.Match match)
			       	                           	{
			       	                           		System.Uri uri = new System.Uri(match.Value);
			       	                           		return uri.GetLeftPart(UriPartial.Path);
			       	                           	})
			       	: result;
		}
	]]>
	</msxsl:script>
	<xsl:key name="failureGroups" match="test-case[@success='False' and @executed = 'True']" use="user:GetMessage(failure/message)"/>
	<xsl:key use="@name" name="machineNames" match="test-case/properties/property[@name='ndistribunit.agent-name']"/>
	<xsl:output method="html" encoding="ISO-8859-1" standalone="yes" version="1.0" indent="yes"/>
	<xsl:param name="applicationPath"/>
	<xsl:template match="/">
		<div id="report">
			<script type="text/javascript">
				var applicationPath = "<xsl:value-of select="$applicationPath"/>";
			</script>
			<script type="text/javascript">
				function expandDiv(eImg, eDiv)
				{
					eDiv.style.display="block";
					eImg.className="button-collapse";
				}
				
				function collapseDiv(eImg, eDiv)
				{
					eDiv.style.display = "none";
					eImg.className="button-expand";
				}
				
				function toggleDiv(imgId, divId)
				{
					eDiv = document.getElementById(divId);
					eImg = document.getElementById(imgId);

					if ( eDiv.style.display == "none" )
					{
						expandDiv(eImg, eDiv);
				 	}
					else
					{
						collapseDiv(eImg, eDiv);
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
				
				function expandAllStackTraces(prefix)
				{
					var i = 1;
					var element = document.getElementById(prefix+i);
					while (element != null)
					{
						expandDiv(document.getElementById("img-"+element.id), element);
						element = document.getElementById(prefix+(++i));
					}
				}
				
				function collapseAllStackTraces(prefix)
				{
					var i = 1;
					var element = document.getElementById(prefix+i);
					while (element != null)
					{
						collapseDiv(document.getElementById("img-"+element.id), element);
						element = document.getElementById(prefix+(++i));
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

				.col0 { width: 20px;min-width: 20px; max-width: 20px; float:left;}
				.col1 { overflow: hidden;}
				.col2 { width: 310px; min-width: 310px; max-width: 310px;float:left;}
				.col3 { width: 40px; text-align: center; min-width: 40px; max-width: 40px; float:left;}

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
				
				.stackTraceToggler { display: inline; }
				.stackTraceArea {background-color: #DDDDDD; border: dotted 1px Gray;}
				

                .button-expand { padding-left: 15px; font-size: 1px; height: 15px; background-repeat: no-repeat;display:inline-block; background-image: url(data:image/gif;base64,R0lGODlhDwAPAPcAAAgICAgIEBAQEBAQGBgYGBgYISEhISEhKSkpMTExMTExOUJCSkpKY1JSUlJSWlJSa1paa1pac2NjY2Njc2Nje2tra3Nzc3NzjISEhIyMjJSUlJycnKWlpbW1tb29vcbGxs7Ozt7e3ufn5+/v7/f39////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////yH5BAEAACUALAAAAAAPAA8AAAiHAEsIFEGChMCDCAWGSIAhxIiECT8AEODgwwiDEEt8ELCAQAMPBDNujPCgQIIOISMKmHBhwgEFHFIeHCniAoUDBjSEwChwo4ifFyBQ/CACIU2bBhBYQGl0JVIEFTJYbBqBQlIJGEDy1EjxAAIJGTzsjBhAwNcMKLf2BJAgqli1CjNo0KAVYkAAADs=);
}
                .button-collapse { padding-left: 15px; font-size: 1px; height: 18px; background-repeat: no-repeat;  display: inline;   display: inline-block; background-image: url(data:image/gif;base64,R0lGODlhDwAPAPcAAAAAAAAACBAQEBAQGBgYGBgYISEhITExMTExOTExQjk5OUJCUkpKUlJSUlJSa1paWmNjY2Njc2tre2trhHNzc3NzjHt7e4SEhKWlpa2trbW1tb29vcbGxs7OztbW1t7e3ufn5+/v7/f39////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////yH5BAEAACMALAAAAAAPAA8AAAiIAEcIHEiwoMGDA0OAWMiwIYgQIj4oMECxokUDED6AUCAgAYOPC0AaCPBAg4gNCAxEqMCwwoIBDyyYDJEBQQEHFSpMSEDgAQUMHUaIAKGhAQEHEhAQgPCzQwiBQzcYJYDRQgYPTwcO5QCBAYULGT5kJSgiRIcMGDZ8EIGwLEO2CKHCjUu3LsGAAAA7);
}				
				
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
			<table class="summaryTable">
				<tbody>
					<tr>
						<th>Failed tests count</th>
						<th>Reason</th>
						<th>Cases</th>
						<th>Machines</th>
					</tr>
					<xsl:for-each select="//test-case[@success='False' and @executed = 'True' and generate-id(.) = generate-id(key('failureGroups', user:GetMessage(failure/message))[1])]">
						<xsl:sort select="count(key('failureGroups', user:GetMessage(failure/message)))" order="descending" data-type="number"/>
						<xsl:variable name="namesDivId">
							<xsl:value-of select="concat(generate-id(@name), '_summary')"/>
						</xsl:variable>
						<xsl:variable name="casesCount">
							<xsl:value-of select="count(key('failureGroups', user:GetMessage(failure/message)))"/>
						</xsl:variable>
						<tr>
							<xsl:if test="$casesCount>50">
								<xsl:attribute name="class">critical</xsl:attribute>
							</xsl:if>
							<xsl:if test="$casesCount>15 and not($casesCount>50)">
								<xsl:attribute name="class">major</xsl:attribute>
							</xsl:if>
							<xsl:if test="$casesCount>3 and not($casesCount>15)">
								<xsl:attribute name="class">medium</xsl:attribute>
							</xsl:if>
							<xsl:if test="not($casesCount>3)">
								<xsl:attribute name="class">minor</xsl:attribute>
							</xsl:if>
							<td class="counts">
								<div>
									<xsl:value-of select="$casesCount"/>
								</div>
							</td>
							<td class="message">
								<div>
									<xsl:value-of select="user:GetMessage(failure/message)"/>
								</div>
							</td>
							<td class="testNames">
								<xsl:element name="div">
								    <xsl:attribute name="class"><xsl:text>clickable</xsl:text></xsl:attribute>
									<xsl:attribute name="onclick"><xsl:text>toggleDiv('img-</xsl:text><xsl:value-of select="$namesDivId"/><xsl:text>','</xsl:text><xsl:value-of select="$namesDivId"/><xsl:text>')</xsl:text></xsl:attribute>
									<xsl:element name="div">							
										<xsl:attribute name="id"><xsl:text>img-</xsl:text><xsl:value-of select="$namesDivId"/></xsl:attribute>
										<xsl:attribute name="class"><xsl:text>button-expand</xsl:text></xsl:attribute>
										<xsl:attribute name="title"><xsl:text>Toggle display of tests failed with such message</xsl:text></xsl:attribute>
										<xsl:text> </xsl:text>
									</xsl:element>
									<xsl:text> Tests</xsl:text>
								</xsl:element>
								<div style="display:none;">
									<xsl:attribute name="id"><xsl:value-of select="$namesDivId"/></xsl:attribute>
									<a href="#">
										<xsl:attribute name="onclick">expandAllStackTraces('<xsl:value-of select="$namesDivId"/>_stackTrace_'); return false;</xsl:attribute>
										Expand All
									</a>
									&#0160;|&#0160;
									<a href="#">
										<xsl:attribute name="onclick">collapseAllStackTraces('<xsl:value-of select="$namesDivId"/>_stackTrace_'); return false;</xsl:attribute>
										Collapse All
									</a><br/>
									
									<xsl:for-each select="key('failureGroups', user:GetMessage(failure/message))">
										<xsl:if test="position()>1">
											<br/>
										</xsl:if>
										<div>
											<xsl:attribute name="class"><xsl:text>clickable stackTraceToggler</xsl:text></xsl:attribute>
											<xsl:attribute name="onclick"><xsl:text>toggleDiv('img-</xsl:text><xsl:value-of select="$namesDivId"/><xsl:text>_stackTrace_</xsl:text><xsl:value-of select="position()"/><xsl:text>','</xsl:text><xsl:value-of select="$namesDivId"/><xsl:text>_stackTrace_</xsl:text><xsl:value-of select="position()"/><xsl:text>')</xsl:text></xsl:attribute>
											<div class="button-expand" title="Toggle display of Stack Trace">
												<xsl:attribute name="id"><xsl:text>img-</xsl:text><xsl:value-of select="$namesDivId"/><xsl:text>_stackTrace_</xsl:text><xsl:value-of select="position()"/><xsl:text></xsl:text></xsl:attribute>
												<xsl:text> </xsl:text>
											</div>
											<xsl:value-of select="position()"/>
											<xsl:text>)&#0160;</xsl:text>
											<xsl:value-of select="@name"/>
											<!--<xsl:text>&#0160; Stack Trace</xsl:text>-->
										</div>
										<div style="display:none;" class="stackTraceArea">
											<xsl:attribute name="id"><xsl:value-of select="$namesDivId"/><xsl:text>_stackTrace_</xsl:text><xsl:value-of select="position()"/><xsl:text></xsl:text></xsl:attribute>
											<pre>
												<xsl:text>Run on machine: </xsl:text><xsl:value-of select="properties/property[@name='ndistribunit.agent-name']/@value"/><br/>
												<xsl:choose>
													<xsl:when test="contains(failure/message, 'TestFixtureSetUp failed')">
														<xsl:value-of select="../../failure/message"/>
														<br/>
														<xsl:value-of select="../../failure/stack-trace"/>
													</xsl:when>
													<xsl:otherwise>
														<xsl:value-of select="failure/message"/>
														<xsl:value-of select="failure/stack-trace"/>
													</xsl:otherwise>
												</xsl:choose>
											</pre>
										</div>
									</xsl:for-each>
								</div>
							</td>
							<td class="machineNames">
								<xsl:call-template name="GetMachineNames">
									<xsl:with-param name="machines" select="key('failureGroups', user:GetMessage(failure/message))/properties/property[@name='ndistribunit.agent-name']"/>
									<xsl:with-param name="index" select="1"/>
									<xsl:with-param name="usedMachineNames"/>
								</xsl:call-template>
							</td>
						</tr>
					</xsl:for-each>
				</tbody>
			</table>
		</div>
	</xsl:template>
	
	<xsl:template name="GetMachineNames">
		<xsl:param name="machines"/>
		<xsl:param name="index"/>
		<xsl:param name="usedMachineNames"/>
<!-- -->
		<xsl:if test="msxsl:node-set($machines)[$index]">
			<xsl:variable name="machineName" select="msxsl:node-set($machines)[$index]/@value"/>
			<xsl:choose>
				<xsl:when test="not(contains($usedMachineNames, $machineName))">
					<xsl:value-of select="$machineName"/>
					<br/>
					<xsl:call-template name="GetMachineNames">
						<xsl:with-param name="machines" select="msxsl:node-set($machines)"/>
						<xsl:with-param name="index" select="$index+1"/>
						<xsl:with-param name="usedMachineNames" select="concat($usedMachineNames, '/|\',$machineName)"/>
					</xsl:call-template>
				</xsl:when>
				<xsl:otherwise>
					<xsl:call-template name="GetMachineNames">
						<xsl:with-param name="machines" select="msxsl:node-set($machines)"/>
						<xsl:with-param name="index" select="$index+1"/>
						<xsl:with-param name="usedMachineNames" select="$usedMachineNames"/>
					</xsl:call-template>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:if>
<!-- -->
	</xsl:template>

</xsl:stylesheet>
