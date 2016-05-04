package buzz.graphicdiff.javers.examples;

import org.javers.core.Javers;
import org.javers.core.JaversBuilder;
import org.javers.core.diff.Diff;
import org.javers.core.metamodel.clazz.ValueObjectDefinition;

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
import gaia.domain.model.ResultDetectionCondition;
import gaia.domain.model.SamplingLocation;
import gaia.domain.model.Specimen;
import gaia.domain.model.Tenant;
import gaia.domain.model.validation.RuleValidationDetails;
import gaia.testcommon.ResourceFactory;

public class ObservationDiff {

    private static final Javers JAVERS = makeObservationJavers();

    private static Javers makeObservationJavers() {
        return JaversBuilder.javers()
                .registerIgnoredClass(AuditAttributes.class)
                .registerIgnoredClass(Tenant.class)
                .registerIgnoredClass(ImportHistoryEvent.class)
                .registerIgnoredClass(RuleValidationDetails.class)
                .registerCustomComparator(new DomainObjectComparator<>(), SamplingLocation.class)
                .registerCustomComparator(new DomainObjectComparator<>(), ObservedProperty.class)
                .registerCustomComparator(new DomainObjectComparator<>(), LabAnalysisMethod.class)
                .registerCustomComparator(new DomainObjectComparator<>(), Activity.class)
                .registerCustomComparator(new DomainObjectComparator<>(), FieldVisit.class)
                .registerCustomComparator(new DomainObjectComparator<>(), Laboratory.class)
                .registerCustomComparator(new DomainObjectComparator<>(), LabReport.class)
                .registerCustomComparator(new SpecimenReferenceComparator(), Specimen.class)
                .registerCustomComparator(new ObjectToStringComparator<>(), DomainDateTime.class)
                .registerValueObject(new ValueObjectDefinition(Observation.class))
                .build();
    }

    private void udpateSamplingLocation(Observation observation, String customId) {
        SamplingLocation updatedLocation = observation.getSamplingLocation().shallowCopy();
        updatedLocation.setCustomId(customId);
        observation.setSamplingLocation(updatedLocation);
    }

    public void diffTwoOfObservations(String threadName) {
        Observation observation1 = ResourceFactory.makeLabObservation();

        Observation observation2 = observation1.shallowCopy();

        udpateSamplingLocation(observation2, "UpdatedLocation");
        observation2.setObservedTime(new DomainDateTime("2010-12-01T13:00:00.000-04:00"));
        observation2.setNumericResult(ResourceFactory.makeNumericResult());
        observation2.getNumericResult().setDetectionCondition(
                ResultDetectionCondition.PRESENT_ABOVE_QUANTIFICATION_LIMIT);

        Diff diff = JAVERS.compare(observation1, observation2);
        System.out.println(threadName + " " + diff);

        String diffAsJson = JAVERS.getJsonConverter().toJson(diff);
        System.out.println(threadName + " Json Diff: \n" + diffAsJson);
    }

    public static void diff() {
        ObservationDiff simpleDiff = new ObservationDiff();
        simpleDiff.diffTwoOfObservations("Thread-Main");
    }

    public static void diffWith10Threads() {
        Runnable task = () -> {
            try {
                String threadName = Thread.currentThread().getName();
                ObservationDiff simpleDiff = new ObservationDiff();
                simpleDiff.diffTwoOfObservations(threadName);
            } catch (Exception cause) {
                cause.printStackTrace(System.err);
            }
        };

        try {
            int numberOfThreads = 10;
            ConcurrencyHelper.execute(task, numberOfThreads);
        } catch (Exception cause) {
            cause.printStackTrace(System.err);
        }
    }

    public static void main(String[] args) {
        diff();
        //diffWith10Threads();
    }
}
