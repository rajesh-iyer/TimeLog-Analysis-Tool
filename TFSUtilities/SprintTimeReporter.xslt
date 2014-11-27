<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
  <xsl:output method="xml" indent="yes"/>

  <xsl:template match="/">
    <html>
      <body style="font-family:Arial, sans-serif;font-size:12px;">
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
          <xsl:for-each select="ArrayOfWorkitemTimeDTO/WorkitemTimeDTO">
            <tr bgcolor="#eeeeee">
              <td><xsl:value-of select="Id"/></td>
              <td><xsl:value-of select="Type"/></td>
              <td><h4><xsl:value-of select="Title"/></h4></td>
              <td><xsl:value-of select="AssignedTo"/></td>
              <td>
                Dev:<xsl:value-of select="PlannedDevEfforts"/> <br />
                QA:<xsl:value-of select="PlannedQAEfforts"/>
              </td>
              <td>
                Dev:<xsl:value-of select="ActualDevEfforts"/>
                QA:<xsl:value-of select="ActualQAEfforts"/>
              </td>
              <td><xsl:value-of select="RemainingWork"/></td>
            </tr>
            <tr>
              <td></td>
              <td>Observations</td>
              <td>
                <ol>
                  <xsl:if test="not(State = 'Committed' or State = 'Done')" >
                    <li style="color:#ff0000">The PBI is not marked Committed or Done.</li>
                  </xsl:if>
                  <xsl:if test="not(PlannedDevEffortEntryGap = 0)" >
                  <li style="color:#ff0000">Workitem and task level planned efforts (Dev) do not match</li>
                </xsl:if>
                <xsl:if test="not(ActualDevEffortEntryGap = 0)" >
                  <li style="color:#ff0000">Workitem and task level actual efforts (Dev) do not match</li>
                </xsl:if>
                  <xsl:if test="not(PlannedQAEffortEntryGap = 0)" >
                    <li style="color:#ff0000">Workitem and task level planned efforts (QA) do not match</li>
                  </xsl:if>
                  <xsl:if test="not(ActualQAEffortEntryGap = 0)" >
                    <li style="color:#ff0000">Workitem and task level actual efforts (QA) do not match</li>
                  </xsl:if>
                  <xsl:if test="HasUnassignedTasks = 'true'" >
                  <li style="color:#ff0000">There are one or more unassigned tasks created for this workitem</li>
                </xsl:if>

                <xsl:if test="(PlannedEfforts = 0)"  >
                  <li style="color:#ff0000">Lifecycle checklist: Estimates are missing</li>
                </xsl:if>
                <xsl:if test="(count(Tasks/WorkitemTimeDTO) = 0)" >
                  <li style="color:#ff0000">Lifecycle checklist: Tasks not created</li>
                </xsl:if>
                <xsl:if test="IsAnalysisTaskCreated = 'false'" >
                  <li style="color:#ff0000">Lifecycle checklist: Impact Analysis task not created yet</li>
                </xsl:if>
                <xsl:if test="IsUnitTestTaskCreated = 'false'" >
                  <li style="color:#ff0000">Lifecycle checklist: Unit testing task not created yet</li>
                </xsl:if>
                <xsl:if test="IsPeerReviewTaskCreated = 'false'" >
                  <li style="color:#ff0000">Lifecycle checklist: Peer code review task not created yet. Please contact respective peer</li>                
              </xsl:if>
                <xsl:if test="IsArchitectReviewTaskCreated = 'false'" >
                  <li style="color:#ff0000">Lifecycle checklist: Architect code review task not created yet. Please contact respective architect/Lead</li>
                </xsl:if>
                <xsl:if test="IsFunctionalTestingTaskCreated = 'false'" >
                  <li style="color:#ff0000">Lifecycle checklist: Functional testing task not created yet. Please contact respective QA</li>                
              </xsl:if>
                  <xsl:if test="IsTCWritingTaskCreated = 'false'" >
                    <li style="color:#ff0000">Lifecycle checklist: TC Writing task not created yet. Please contact respective QA</li>
                  </xsl:if>
                  <xsl:if test="IsTaskMarkedAsDoneButNoTime = 'true'" >
                  <li style="color:#ff0000">There is atleast one task marked completed, but no time logged under it</li>                
              </xsl:if>
                  <xsl:if test="AnyTaskTitleMissingPBINumber = 'true'" >
                    <li style="color:#ff0000">At least one task title does not start with PBI number</li>
                  </xsl:if>

                  <xsl:if test="DevTaskActivityNotMatching = 'true'" >
                    <li style="color:#ff0000">Lifecycle checklist: One or more developer tasks do not have the right "Activity" (Development)</li>
                  </xsl:if>
                  <xsl:if test="QATaskActivityNotMatching = 'true'" >
                    <li style="color:#ff0000">Lifecycle checklist: One or more tester tasks do not have the right "Activity" (Testing)</li>
                  </xsl:if>
                  <xsl:if test="ReviewTaskActivityNotMatching = 'true'" >
                    <li style="color:#ff0000">Lifecycle checklist: One or more review tasks do not have the right "Activity" (Design)</li>
                  </xsl:if>

                  <!--<xsl:if test="ExceedingTimeToComplete &gt; 0" >
                  <li style="color:#ff0000">Workitem may be behind schedule. You are falling short by <xsl:value-of select="ExceedingTimeToComplete"/> hours</li>
                </xsl:if>-->
                </ol>
              </td>
            </tr>
            <xsl:for-each select="Tasks/WorkitemTimeDTO">
              <xsl:choose>
                <xsl:when test="IsTaskMarkedAsDone = 'true'">
                  <tr bgcolor="#80FF80">
                    <td><xsl:value-of select="Id"/></td>
                    <td><xsl:value-of select="Type"/></td>
                    <td><xsl:value-of select="Title"/></td>
                    <td><xsl:value-of select="AssignedTo"/></td>
                    <td><xsl:value-of select="PlannedDevEfforts"/></td>
                    <td><xsl:value-of select="ActualDevEfforts"/></td>
                    <td><xsl:value-of select="RemainingWork"/></td>
                  </tr>
                </xsl:when>
                <xsl:otherwise>
                  <tr>
                    <td><xsl:value-of select="Id"/></td>
                    <td><xsl:value-of select="Type"/></td>
                    <td><xsl:value-of select="Title"/></td>
                    <td><xsl:value-of select="AssignedTo"/></td>
                    <td><xsl:value-of select="PlannedDevEfforts"/></td>
                    <td><xsl:value-of select="ActualDevEfforts"/></td>
                    <td><xsl:value-of select="RemainingWork"/></td>
                  </tr>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:for-each>
          </xsl:for-each>
        </table>
        <br />
        <div style="border:#aaa 1px solid;background-color:#f0f0f0;margin:30px;padding:30px;">
          <div>
            <b>General Rules</b>
            <ul>
              <li>Add the User story number or Bug number in the title of the Sub task</li>
              <li>It is mandatory to select “Activity” field while creating task
                  <ul>
                    <li>Select <b>Development</b> for all Developer tasks</li>
                    <li>Select <b>Testing</b> for all QA tasks</li>
                    <li>Select <b>Design</b> for all Review related tasks</li>
                  </ul>
              </li>
            </ul>
            <br />
            <b>Dev Tasks</b>
            <ul>
              <li>While creating Analysis task use the name “Impact Analysis”. Use exact text.</li>
              <li>While creating Peer code review task use the name “Peer Code Review” . Use exact text.</li>
              <li>While creating Architect code review task use the name “Architect Code Review” . Use exact text.</li>
            </ul>

            <br />
            <b>QA Tasks</b>
            <ul>
              <li>While creating RS, TC creation task use the name “TC Writing”. Use exact text.</li>
              <li>While creating Testing task use the name “Functional Testing”. Use exact text.</li>
            </ul>
          </div>
        </div>
      </body>
    </html>
  </xsl:template>
</xsl:stylesheet>

