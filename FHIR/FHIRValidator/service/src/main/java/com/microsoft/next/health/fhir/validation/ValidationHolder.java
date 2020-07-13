/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package com.microsoft.next.health.fhir.validation;

import org.hl7.fhir.r5.model.FhirPublication;
import org.hl7.fhir.validation.ValidationEngine;

/**
 *
 * @author stordahl
 */
public class ValidationHolder {
    
      // static variable single_instance of type Singleton 
    private static ValidationEngine single_instance = null; 
  
   
    // private constructor restricted to this class itself 
    private ValidationHolder() 
    { 
       
    } 
  
    // static method to create instance of Singleton class 
    public static ValidationEngine getEngineInstance() 
    { 
        
        if (single_instance == null) {
            
            try {
                single_instance = new ValidationEngine("hl7.fhir.r4.core#4.0.1");
                single_instance.connectToTSServer("http://tx.fhir.org", new String(), FhirPublication.R4);
                single_instance.loadIg("hl7.fhir.us.core", true);
               
                
            }
            catch (Exception e) {
                e.printStackTrace(System.out);
            }
        }
  
        return single_instance; 
    } 
    
}
