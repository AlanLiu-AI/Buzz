package gaia.graphicdiff.examples;

import java.util.ArrayList;
import java.util.List;

import org.joda.time.Instant;

public abstract class ConcurrencyHelper {

    public static void execute(Runnable task, int numberOfThreads) {
        try {
            List<Thread> threads = new ArrayList<>();
            for (int index = 0; index < numberOfThreads; index++) {
                threads.add(new Thread(task));
            }

            Instant startTime = Instant.now();
            for (int index = 0; index < numberOfThreads; index++) {
                threads.get(index).start();
            }

            boolean shouldWaitThreadsDone = true;
            while(shouldWaitThreadsDone) {
                shouldWaitThreadsDone = false;
                for (int index = 0; index < numberOfThreads; index++) {
                    if (threads.get(index).isAlive()) {
                        shouldWaitThreadsDone = true;
                        break;
                    }
                }
            }
            long duration = Instant.now().getMillis() - startTime.getMillis();
            System.out.println(String.format("ConcurrencyHelper execute done with %s threads, duration %d ms.",
                    numberOfThreads, duration));
        } catch (Exception cause) {
            cause.printStackTrace(System.err);
        }

    }
}
