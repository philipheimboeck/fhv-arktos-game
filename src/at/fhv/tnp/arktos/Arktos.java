package at.fhv.tnp.arktos;

import at.fhv.tnp.arktos.controller.JInputDevice;

/**
 * Author: Philip Heimböck
 * Date: 15.10.15.
 */
public class Arktos {

    public static void main(String[] args) {
        JInputDevice inputDevice = JInputDevice.getDevice(JInputDevice.JInputDeviceType.Gamepad);

        if (inputDevice != null) {
            inputDevice.init();

            System.out.println("\nStarting...\n");

            while (true) {
                inputDevice.update();
            }
        }
    }

}
