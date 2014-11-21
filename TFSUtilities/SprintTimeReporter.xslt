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
            <th width="5%">Planned</th>
            <th width="5%">Actual</th>
            <th width="5%">Remaining</th>
          </tr>
          <xsl:for-each select="ArrayOfWorkitemTime/WorkitemTime">
            <tr bgcolor="#eeeeee">
              <td><xsl:value-of select="Id"/></td>
              <td><xsl:value-of select="Type"/></td>
              <td><xsl:value-of select="Title"/>
                <xsl:if test="not(PlannedEffortEntryGap = 0)" >
                  <div style="color:#ff0000">Workitem and task level planned efforts do not match</div>
                </xsl:if>
                <xsl:if test="not(ActualEffortEntryGap = 0)" >
                  <div style="color:#ff0000">Workitem and task level actual efforts do not match</div>
                </xsl:if>
                <xsl:if test="not(RemainingEffortEntryGap = 0)" >
                  <div style="color:#ff0000">Workitem and task level remaining efforts do not match</div>
                </xsl:if>
                
                <xsl:if test="HasUnassignedTasks = 'false'" >
                  <div style="color:#ff0000">There are one or more unassigned tasks created for this workitem</div>
                </xsl:if>

                <xsl:if test="(PlannedEfforts = 0)"  >
                  <div style="color:#ff0000">Lifecycle checklist: Estimates are missing</div>
                </xsl:if>
                <xsl:if test="(count(Tasks/WorkitemTime) = 0)" >
                  <div style="color:#ff0000">Lifecycle checklist: Tasks not created</div>
                </xsl:if>

                <xsl:if test="IsAnalysisTaskCreated = 'false'" >
                  <div style="color:#ff0000">Lifecycle checklist: Analysis task not created yet.</div>
                </xsl:if>
                <xsl:if test="IsUnitTestTaskCreated = 'false'" >
                  <div style="color:#ff0000">Lifecycle checklist: Unit testing task not created yet.</div>
                </xsl:if>
                <xsl:if test="IsPeerReviewTaskCreated = 'false'" >
                  <div style="color:#ff0000">Lifecycle checklist: Peer review task not created yet.</div>
                </xsl:if>
                <xsl:if test="IsArchitectReviewTaskCreated = 'false'" >
                  <div style="color:#ff0000">Lifecycle checklist: Architect review task not created yet.</div>
                </xsl:if>
                <xsl:if test="IsTestingTaskCreated = 'false'" >
                  <div style="color:#ff0000">Lifecycle checklist: QA testing task not created yet.</div>
                </xsl:if>

                <xsl:if test="IsTaskMarkedAsDoneButNoTime = 'true'" >
                  <div style="color:#ff0000">There is atleast one task marked completed, but no time logged under it.</div>
                </xsl:if>

                <!--<xsl:if test="ExceedingTimeToComplete &gt; 0" >
                  <div style="color:#ff0000">Workitem may be behind schedule. You are falling short by <xsl:value-of select="ExceedingTimeToComplete"/> hours</div>
                </xsl:if>-->              
              </td>
              <td><xsl:value-of select="PlannedEfforts"/></td>
              <td><xsl:value-of select="ActualEfforts"/></td>
              <td><xsl:value-of select="RemainingWork"/></td>
            </tr>
            <xsl:for-each select="Tasks/WorkitemTime">
              <xsl:choose>
                <xsl:when test="IsTaskMarkedAsDone = 'true'">
                  <tr bgcolor="#80FF80">
                    <td><xsl:value-of select="Id"/></td>
                    <td><xsl:value-of select="Type"/></td>
                    <td><xsl:value-of select="Title"/></td>
                    <td><xsl:value-of select="PlannedEfforts"/></td>
                    <td><xsl:value-of select="ActualEfforts"/></td>
                    <td><xsl:value-of select="RemainingWork"/></td>
                  </tr>
                </xsl:when>
                <xsl:otherwise>
                  <tr>
                    <td><xsl:value-of select="Id"/></td>
                    <td><xsl:value-of select="Type"/></td>
                    <td><xsl:value-of select="Title"/></td>
                    <td><xsl:value-of select="PlannedEfforts"/></td>
                    <td><xsl:value-of select="ActualEfforts"/></td>
                    <td><xsl:value-of select="RemainingWork"/></td>
                  </tr>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:for-each>          
          </xsl:for-each>
        </table>
      </body>
    </html>
  </xsl:template>
</xsl:stylesheet>

