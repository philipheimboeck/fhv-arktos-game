package at.fhv.tnp.arktos.controller;

import net.java.games.input.Component;
import net.java.games.input.Controller;
import net.java.games.input.ControllerEnvironment;
import net.java.games.input.Event;

import java.util.HashMap;
import java.util.LinkedList;
import java.util.Queue;

/**
 * Author: Philip Heimböck
 * Date: 15.10.15.
 */
public class JInputDevice extends InputDevice {

    public enum JInputDeviceType {
        Gamepad,
        Keyboard,
        Mouse
    }

    private static HashMap<JInputDeviceType, Queue<Controller>> sDevices = new HashMap<>();

    static {
        refreshDevices();
    }

    public static void refreshDevices() {
        // Clear the device list
        sDevices.clear();

        sDevices.put(JInputDeviceType.Gamepad, new LinkedList<Controller>());
        sDevices.put(JInputDeviceType.Keyboard, new LinkedList<Controller>());
        sDevices.put(JInputDeviceType.Mouse, new LinkedList<Controller>());

        // Add all controllers to the list
        Controller[] controllers = ControllerEnvironment.getDefaultEnvironment().getControllers();
        for (Controller c : controllers) {
            if (c.getType().equals(Controller.Type.GAMEPAD)) {
                sDevices.get(JInputDeviceType.Gamepad).add(c);
            } else if (c.getType().equals(Controller.Type.KEYBOARD)) {
                sDevices.get(JInputDeviceType.Keyboard).add(c);
            } else if (c.getType().equals(Controller.Type.MOUSE)) {
                sDevices.get(JInputDeviceType.Mouse).add(c);
            }
        }
    }

    public static JInputDevice getDevice(JInputDeviceType type) {
        if (!sDevices.get(type).isEmpty()) {
            Controller controller = sDevices.get(type).poll();
            return new JInputDevice(controller);
        }

        return null;
    }

    private Controller pController;

    private JInputDevice(Controller controller) {
        pController = controller;
    }

    public void init() {
        for (Component c : pController.getComponents()) {
            System.out.println(c.getName() + " Analog: " + c.isAnalog() + " Relative: " + c.isRelative() + " ID: " + c.getIdentifier().getName());
        }
    }

    @Override
    public void update() {
        pController.poll();

        Event event = new Event();
        if (pController.getEventQueue().getNextEvent(event)) {
            System.out.println(event.getComponent().getName() + " : " + event.getValue());
        }
    }
}
