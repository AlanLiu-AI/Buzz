package buzz.graphicdiff.javers.examples;

import java.util.ArrayList;
import java.util.List;

public abstract class ConcurrencyHelper {

    public static void execute(Runnable task, int numberOfThreads) {
        try {
            List<Thread> threads = new ArrayList<>();
            for (int index = 0; index < numberOfThreads; index++) {
                threads.add(new Thread(task));
            }

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
            System.out.println("ConcurrencyHelper execute done with " + numberOfThreads + " threads.");
        } catch (Exception cause) {
            cause.printStackTrace(System.err);
        }

    }
}
