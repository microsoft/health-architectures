package com.microsoft.next.health.fhir.validation;

import org.hl7.fhir.r5.formats.JsonParser;
import java.io.BufferedReader;
import java.io.IOException;
import java.util.ArrayList;
import java.util.Enumeration;

import javax.servlet.ServletException;
import javax.servlet.ServletOutputStream;
import javax.servlet.annotation.WebServlet;
import javax.servlet.http.HttpServlet;
import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;
import org.hl7.fhir.r5.elementmodel.Manager;
import org.hl7.fhir.r5.formats.IParser;
import org.hl7.fhir.r5.model.OperationOutcome;

@WebServlet(
        name = "FHIRValidator",
        urlPatterns = {"/validate"}
    )
public class ValidationServlet extends HttpServlet {

    @Override
   public void doPost(HttpServletRequest request, HttpServletResponse response)
      throws ServletException, IOException {
       
        //Load Profiles to validate against
        ArrayList<String> profiles = new ArrayList<String>();
        String[] paramValues = request.getParameterValues("profile");
        if (paramValues != null) {
            for (int i = 0; i < paramValues.length; i++) {
                String paramValue = paramValues[i];
                profiles.add(paramValue);
            }
        }
        StringBuilder jb = new StringBuilder();
        String line = null;
         
        try {
            BufferedReader reader = request.getReader();
            while ((line = reader.readLine()) != null)
                jb.append(line);
            response.setContentType("application/json");
            OperationOutcome oo = ValidationHolder.getEngineInstance().validate(jb.toString().getBytes(), Manager.FhirFormat.JSON, profiles);
            System.out.println("Issues: " + oo.getIssue().size());
            int error=0,warn=0,info=0;
            for (OperationOutcome.OperationOutcomeIssueComponent issue : oo.getIssue()) {
                if (issue.getSeverity() == OperationOutcome.IssueSeverity.FATAL || issue.getSeverity() == OperationOutcome.IssueSeverity.ERROR)
                    error++;
                else if (issue.getSeverity() == OperationOutcome.IssueSeverity.WARNING)
                    warn++;
                else
                    info++;
            }
            System.out.println((error == 0 ? "Success..." : "*FAILURE* ") + "validating: " + " error:" + Integer.toString(error) + " warn:" + Integer.toString(warn) + " info:" + Integer.toString(info));
            ServletOutputStream out = response.getOutputStream();
            JsonParser x = new JsonParser();
            x.setOutputStyle(IParser.OutputStyle.NORMAL);
            x.compose(out, oo);
           
        } catch (Exception e) { 
            e.printStackTrace(System.out); 
        }

   }   

}