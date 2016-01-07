package buzz.graphicdiff.javers.examples;

import org.javers.core.Javers;
import org.javers.core.JaversBuilder;
import org.javers.core.diff.Diff;
import org.javers.core.metamodel.clazz.EntityDefinition;
import org.javers.core.metamodel.clazz.EntityDefinitionBuilder;

import gaia.domain.model.Activity;
import gaia.domain.model.DomainDateTime;
import gaia.domain.model.FieldVisit;
import gaia.domain.model.Observation;
import gaia.domain.model.ObservedProperty;
import gaia.domain.model.SamplingLocation;
import gaia.domain.model.Specimen;
import gaia.testcommon.ResourceFactory;

public class SimpleDiff {

    public Javers makeObservationJavers() {

        EntityDefinition entityDefinition = EntityDefinitionBuilder.entityDefinition(Observation.class)
                .withIdPropertyName("id")
                .withIgnoredProperties("tenant")
                .withIgnoredProperties("auditAttribitues")
                .withIgnoredProperties("validationWarnings")
                .build();

        return JaversBuilder.javers()
                .registerCustomComparator(new DomainObjectComparator<SamplingLocation>(), SamplingLocation.class)
                .registerCustomComparator(new DomainObjectComparator<ObservedProperty>(), ObservedProperty.class)
                .registerCustomComparator(new DomainObjectComparator<Activity>(), Activity.class)
                .registerCustomComparator(new DomainObjectComparator<FieldVisit>(), FieldVisit.class)
                .registerCustomComparator(new SpecimenReferenceComparator(), Specimen.class)
                .registerCustomComparator(new ObjectToStringComparator<DomainDateTime>(), DomainDateTime.class)
                .registerEntity(entityDefinition)
                .build();
    }

    public void diffObservations() {

        Observation observation1 = ResourceFactory.makeLabObservation();
        Observation observation2 = observation1.shallowCopy();
        SamplingLocation updatedLocation = observation1.getSamplingLocation().shallowCopy();
        updatedLocation.setCustomId("newLocation");
        observation2.setSamplingLocation(updatedLocation);
        observation2.setObservedTime(new DomainDateTime("2010-12-01T13:00:00.000-04:00"));

        Javers javers = makeObservationJavers();

        Diff diff = javers.compare(observation1, observation2);

        System.out.println(diff);
        /* console output:
Diff:
1. ValueChange{globalId:'gaia.domain.model.Observation/6575174818313093369,-9093171716606314211', property:'samplingLocation', oldVal:'TESTLOC00220160504144706251', newVal:'newLocation'}
2. ValueChange{globalId:'gaia.domain.model.Observation/6575174818313093369,-9093171716606314211', property:'observedTime', oldVal:'1973-09-11T11:55:00.000-03:00', newVal:'2010-12-01T13:00:00.000-04:00'}
        */
    }

    public void diffAll() {
        diffObservations();
    }

    public static void main(String[] args) {
        SimpleDiff simpleDiff = new SimpleDiff();

        try {
            simpleDiff.diffAll();
        } catch (Exception cause) {
            cause.printStackTrace(System.err);
        }
    }
}
