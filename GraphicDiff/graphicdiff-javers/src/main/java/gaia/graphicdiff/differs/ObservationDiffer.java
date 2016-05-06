package gaia.graphicdiff.differs;

import java.util.ArrayList;
import java.util.List;
import java.util.regex.Pattern;

import org.javers.core.Javers;
import org.javers.core.JaversBuilder;
import org.javers.core.diff.Change;
import org.javers.core.diff.Diff;
import org.javers.core.diff.changetype.ValueChange;
import org.javers.core.metamodel.clazz.ValueObjectDefinition;
import org.javers.core.metamodel.clazz.ValueObjectDefinitionBuilder;
import org.javers.core.metamodel.object.GlobalId;

import gaia.domain.model.Activity;
import gaia.domain.model.AuditAttributes;
import gaia.domain.model.DomainDateTime;
import gaia.domain.model.FieldVisit;
import gaia.domain.model.ImportHistoryEvent;
import gaia.domain.model.LabAnalysisMethod;
import gaia.domain.model.LabReport;
import gaia.domain.model.Laboratory;
import gaia.domain.model.Observation;
import gaia.domain.model.ObservedProperty;
import gaia.domain.model.SamplingLocation;
import gaia.domain.model.Specimen;
import gaia.domain.model.Tenant;
import gaia.domain.model.validation.RuleValidationDetails;
import gaia.graphicdiff.differs.comarators.ObjectToStringComparator;
import gaia.graphicdiff.differs.comarators.SinglePropertyToStringComparator;

public class ObservationDiffer
    implements IDomainObjectDiffer<Observation> {

    private static final Javers JAVERS = makeObservationJavers();
    private static final Pattern JAVERS_KEY_SEPRATOR_REGEX = Pattern.compile("/#|//|/");

    private static Javers makeObservationJavers() {
        ValueObjectDefinition observationDefinition = ValueObjectDefinitionBuilder
                .valueObjectDefinition(Observation.class)
                .withTypeName(Observation.class.getSimpleName())
                .withIgnoredProperties()
                .build();

        return JaversBuilder.javers()
                .registerIgnoredClass(AuditAttributes.class)
                .registerIgnoredClass(Tenant.class)
                .registerIgnoredClass(ImportHistoryEvent.class)
                .registerIgnoredClass(RuleValidationDetails.class)
                .registerCustomComparator(SinglePropertyToStringComparator.createComparator(SamplingLocation.class, "customId"), SamplingLocation.class)
                .registerCustomComparator(SinglePropertyToStringComparator.createComparator(ObservedProperty.class, "customId"), ObservedProperty.class)
                .registerCustomComparator(SinglePropertyToStringComparator.createComparator(LabAnalysisMethod.class, "customId"), LabAnalysisMethod.class)
                .registerCustomComparator(SinglePropertyToStringComparator.createComparator(Activity.class, "customId"), Activity.class)
                .registerCustomComparator(SinglePropertyToStringComparator.createComparator(FieldVisit.class, "customId"), FieldVisit.class)
                .registerCustomComparator(SinglePropertyToStringComparator.createComparator(Laboratory.class, "customId"), Laboratory.class)
                .registerCustomComparator(SinglePropertyToStringComparator.createComparator(LabReport.class, "customId"), LabReport.class)
                .registerCustomComparator(SinglePropertyToStringComparator.createComparator(Specimen.class, "name"), Specimen.class)
                .registerCustomComparator(new ObjectToStringComparator<>(), DomainDateTime.class)
                .registerValueObject(observationDefinition)
                .build();
    }

    public List<FieldChange> getFieldChanges(Observation observation1, Observation observation2) {

        Diff diff = JAVERS.compare(observation1, observation2);
        //System.out.println(diff);

        //String diffAsJson = JAVERS.getJsonConverter().toJson(diff);
        //System.out.println("Json Diff: \n" + diffAsJson);

        return transform(diff);
    }

    private List<FieldChange> transform(Diff diff) {

        List<FieldChange> fieldChanges = new ArrayList<FieldChange>();

        for (Change change: diff.getChanges()) {

            if (change instanceof ValueChange) {
                ValueChange valueChange = (ValueChange)change;

                String key = composeFieldKey(valueChange.getAffectedGlobalId(), valueChange.getPropertyName());
                String oldValue = valueChange.getLeft().toString();
                String newValue = valueChange.getRight().toString();

                fieldChanges.add(new FieldChange(key, oldValue, newValue));
            }
        }

        return fieldChanges;
    }

    private String composeFieldKey(GlobalId globalId, String propertyName) {
        String javerKey = globalId.toString() + "/" + propertyName;

        return JAVERS_KEY_SEPRATOR_REGEX.matcher(javerKey).replaceAll("_");
    }
}
