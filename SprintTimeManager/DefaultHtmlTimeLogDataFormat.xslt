<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
  <xsl:output method="xml" indent="yes"/>

  <xsl:template match="/TimeLogData">
    <html>
      <body style="font-family:Arial, sans-serif;font-size:12px;">

        <div style='font-family:Arial, sans-serif;font-size:12px;'>
          <xsl:value-of select="TeamMember/Fullname"/>, <br /><br />
          You today's task status for <i>
            <xsl:value-of select="ServiceContext/IterationPath"/>
          </i>.
          <br /><br />
          <b>The sprint milestones:</b>
          <ol>
            <xsl:for-each select="ServiceContext/Milestones/Milestone">
              <li>
                <xsl:value-of select="Title"/> : <xsl:value-of select="substring-before(CompletionDate,'T')"/>
              </li>
            </xsl:for-each>
          </ol>
        </div>
        <h3>Daily work summary</h3>
        <table border="0" style="font-family:Arial, sans-serif;font-size:12px;">
          <tr>
            <th width="5%">Id</th>
            <th width="10%">Type</th>
            <th>Title</th>
            <th width="10%">AssignedTo</th>
            <th width="2%">Planned</th>
            <th width="2%">Actual</th>
            <th width="2%">Remaining</th>
          </tr>
          <xsl:for-each select="Workitems/TimeLog">
            <tr bgcolor="#00B0F0" style="color:#fff;">
              <td>
                <xsl:value-of select="WorkitemId"/>
              </td>
              <td>
                <xsl:value-of select="Type"/>
              </td>
              <td>
                <h4>
                  <xsl:value-of select="Title"/>
                  <i>
                    (<xsl:value-of select="DevelopmentTracking"/>)
                  </i>
                </h4>
              </td>
              <td>
                <xsl:value-of select="AssignedTo"/>
              </td>
              <td>
                Dev:<xsl:value-of select="PlannedDevEfforts"/> <br />
                QA:<xsl:value-of select="PlannedQAEfforts"/>
              </td>
              <td>
                Dev:<xsl:value-of select="ActualDevEfforts"/>
                QA:<xsl:value-of select="ActualQAEfforts"/>
              </td>
              <td>
                <xsl:value-of select="RemainingWork"/>
              </td>
            </tr>
            <xsl:if test="not(count(Observations/Observation) = 0)" >
              <tr bgcolor="#f0f0f0">
                <td></td>
                <td style="valign:top;">Observations</td>
                <td>
                  <ol style="color:#f00;">
                    <xsl:for-each select="Observations/Observation">
                      <xsl:choose>
                        <xsl:when test="Code = 'Milestone'">
                          <li style="background-color:#F00;color:#FFF;">
                            <xsl:value-of select="Code"/> : <xsl:value-of select="Title"/>
                          </li>
                        </xsl:when>
                        <xsl:otherwise>
                          <li>
                            <xsl:value-of select="Code"/> : <xsl:value-of select="Title"/>
                          </li>
                        </xsl:otherwise>
                      </xsl:choose>
                    </xsl:for-each>
                  </ol>
                </td>
              </tr>
            </xsl:if>
            <xsl:for-each select="Tasks/TimeLog">
              <xsl:choose>
                <xsl:when test="IsTaskMarkedAsDone = 'true'">
                  <tr bgcolor="#80FF80">
                    <td>
                      <xsl:value-of select="WorkitemId"/>
                    </td>
                    <td>
                      <xsl:value-of select="Type"/>
                    </td>
                    <td>
                      <xsl:value-of select="Title"/>
                      (<xsl:value-of select="Activity"/>)
                    </td>
                    <td>
                      <xsl:value-of select="AssignedTo"/>
                    </td>
                    <td>
                      <xsl:value-of select="PlannedDevEfforts"/>
                    </td>
                    <td>
                      <xsl:value-of select="ActualDevEfforts"/>
                    </td>
                    <td>
                      <xsl:value-of select="RemainingWork"/>
                    </td>
                  </tr>
                </xsl:when>
                <xsl:otherwise>
                  <tr>
                    <td style="border-bottom:solid 1px #ddd;">
                      <xsl:value-of select="WorkitemId"/>
                    </td>
                    <td style="border-bottom:solid 1px #ddd;">
                      <xsl:value-of select="Type"/>
                    </td>
                    <td style="border-bottom:solid 1px #ddd;">
                      <xsl:value-of select="Title"/>
                      (<xsl:value-of select="Activity"/>)
                    </td>
                    <td style="border-bottom:solid 1px #ddd;">
                      <xsl:value-of select="AssignedTo"/>
                    </td>
                    <td style="border-bottom:solid 1px #ddd;">
                      <xsl:value-of select="PlannedDevEfforts"/>
                    </td>
                    <td style="border-bottom:solid 1px #ddd;">
                      <xsl:value-of select="ActualDevEfforts"/>
                    </td>
                    <td style="border-bottom:solid 1px #ddd;">
                      <xsl:value-of select="RemainingWork"/>
                    </td>
                  </tr>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:for-each>
            <tr>
              <td></td>
            </tr>
          </xsl:for-each>
        </table>
        <br />
        <div style="border:#aaa 1px solid;margin:30px;padding:30px;">
          <div>
            <ul>
              <li>
                <b>General Rules</b>
                <ul>
                  <li>Add the User story number or Bug number in the title of the Sub task</li>
                  <li>All the Sub tasks must be in the same sprint as Parent</li>
                  <li>
                    It is mandatory to select “Activity” field while creating task
                    <ul>
                      <li>
                        Select <b>Development</b> for all Developer tasks
                      </li>
                      <li>
                        Select <b>Testing</b> for all QA tasks
                      </li>
                      <li>
                        Select <b>Design</b> for all Review related tasks
                      </li>
                    </ul>
                  </li>
                  <li>Only Developer and QA to update efforts at Story level </li>
                </ul>
              </li>

              <li>
                <b>User stories</b>
                <ul>
                  <li>
                    <b>Dev tasks</b>
                    <ul>
                      <li>While creating Analysis task use the name “Impact Analysis”. Use exact text.</li>
                      <li>While creating Peer code review task use the name “Peer Code Review” . Use exact text.</li>
                      <li>While creating Architect code review task use the name “Architect Code Review” . Use exact text.</li>
                    </ul>
                  </li>
                  <li>
                    <b>QA tasks</b>
                    <ul>
                      <li>While creating RS, TC creation task use the name “TC Writing”. Use exact text.</li>
                      <li>While creating Testing task use the name “Functional Testing”. Use exact text.</li>
                    </ul>
                  </li>
                </ul>
              </li>
              <li>
                <b>Bugs</b>
                <ul>
                  <li>
                    <b>Dev tasks</b>
                    <ul>
                      <li>While creating Coding task use the name “Coding”. Use exact text.</li>
                    </ul>
                  </li>
                  <li>
                    <b>QA tasks</b>
                    <ul>
                      <li>While creating Testing task use the name “Functional Testing”. Use exact text.</li>
                    </ul>
                  </li>
                </ul>
              </li>
            </ul>
          </div>
        </div>
      </body>
    </html>
  </xsl:template>
</xsl:stylesheet>

