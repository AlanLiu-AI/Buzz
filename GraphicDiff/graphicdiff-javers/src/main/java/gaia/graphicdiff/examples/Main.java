package gaia.graphicdiff.examples;

import java.util.List;

import com.google.common.base.Strings;

import gaia.graphicdiff.differs.FieldChange;
import gaia.graphicdiff.differs.ObservationDiffer;

import gaia.domain.model.DomainDateTime;
import gaia.domain.model.Observation;
import gaia.domain.model.ResultDetectionCondition;
import gaia.domain.model.SamplingLocation;
import gaia.testcommon.ResourceFactory;

public class Main {

    private void udpateSamplingLocation(Observation observation, String customId) {
        SamplingLocation updatedLocation = observation.getSamplingLocation().shallowCopy();
        updatedLocation.setCustomId(customId);
        observation.setSamplingLocation(updatedLocation);
    }

    public void diff(String threadName, int numberOfCallPerThread) {
        for (int callNumber = 0; callNumber < numberOfCallPerThread; callNumber++) {
            randomDiff(threadName, callNumber+1);
        }
    }

    public void randomDiff(String threadName, int callNumber) {

        Observation observation1 = ResourceFactory.makeLabObservation();

        Observation observation2 = observation1.shallowCopy();

        udpateSamplingLocation(observation2, "UpdatedLocation" + callNumber);

        DomainDateTime updatedObservedTime = new DomainDateTime("2010-12-01T13:00:00.000-04:00");
        updatedObservedTime.getDateTime().plusHours(callNumber);
        observation2.setObservedTime(updatedObservedTime);

        observation2.setNumericResult(ResourceFactory.makeNumericResult());

        observation2.getNumericResult().setDetectionCondition(
                ResultDetectionCondition.PRESENT_ABOVE_QUANTIFICATION_LIMIT);

        ObservationDiffer observationDiff = new ObservationDiffer();
        List<FieldChange> fieldChanges = observationDiff.getFieldChanges(observation1, observation2);

        System.out.println(String.format("[%s][%2s] fieldChanges:", threadName, callNumber));
        for (int index = 0; index < fieldChanges.size(); index++) {
            System.out.println(String.format("[%s][%2s] %d: %s", threadName, callNumber, index+1,
                    fieldChanges.get(index)));
        }
    }

    public static void diffMain(int numberOfCallPerThread) {
        Main simpleDiff = new Main();
        simpleDiff.diff("Thread-Main", numberOfCallPerThread);
    }

    public static void diffWithMultiThreads(int numberOfThreads, int numberOfCallPerThread) {
        Runnable task = () -> {
            try {
                String threadName = Thread.currentThread().getName();
                Main simpleDiff = new Main();
                simpleDiff.diff(threadName, numberOfCallPerThread);
            } catch (Exception cause) {
                cause.printStackTrace(System.err);
            }
        };

        try {
            ConcurrencyHelper.execute(task, numberOfThreads);
        } catch (Exception cause) {
            cause.printStackTrace(System.err);
        }
    }

    public static void main(String[] args) {
        String multiThreadsProp = System.getProperty("multiThreads");
        String callPerThreadProp = System.getProperty("callPerThread");
        int numberOfThreads = Strings.isNullOrEmpty(multiThreadsProp) ? 1 : Integer.valueOf(multiThreadsProp);
        int callPerThread = Strings.isNullOrEmpty(callPerThreadProp) ? 1 : Integer.valueOf(callPerThreadProp);

        if (numberOfThreads > 1) {
            diffWithMultiThreads(numberOfThreads, callPerThread);
        } else {
            diffMain(callPerThread);
        }
    }
}
