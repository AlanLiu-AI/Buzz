package buzz.misc.groovy.examples;

import java.io.InputStream;
import java.io.InputStreamReader;
import java.util.HashMap;
import java.util.Map;

import com.google.common.base.Charsets;
import com.google.common.io.CharStreams;
import groovy.lang.Binding;
import groovy.lang.GroovyShell;

public class Main {



    public static void main(String[] args) {

        System.out.println("welcome.");

        try {
            String groovyScript;
            try (InputStream inputStream = Main.class.getClassLoader()
                    .getResourceAsStream("print.groovy");
                 InputStreamReader inputStreamReader = new InputStreamReader(inputStream, Charsets.UTF_8)) {
                groovyScript = CharStreams.toString(inputStreamReader);
            }

            Map<String, String> properties = new HashMap<>();
            properties.put("var1", "var1");
            properties.put("var2", "var2");
            properties.put("var3", "var3");

            Binding binding = new Binding();
            binding.setVariable("properties", properties);

            GroovyShell shell = new GroovyShell(binding);
            Object returnValue = shell.evaluate(groovyScript);

            System.out.println(returnValue.toString());
        } catch (Exception cause) {
            cause.printStackTrace(System.err);
        }

    }
}
