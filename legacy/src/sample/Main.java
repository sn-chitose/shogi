package sample;

import javax.swing.*;
import java.awt.*;
import java.awt.event.*;

public class Main extends JFrame {

    private ShogiPlayer player;

    public Main(Color front, Color back) {
        this(front, back, back);
    }

    private Main(Color front, Color back, Color tileback) {
        super("ShogiPlayer");
        setDefaultCloseOperation(WindowConstants.EXIT_ON_CLOSE);
        setResizable(false);
        setSize(800, 600);
        setBackground(back);
        player = new ShogiPlayer(front, back, tileback);
        add(player);
        player.addNotify();
    }

    public static void main(String[] args) {
        Main main = new Main(Color.white, Color.darkGray, Color.black);
        main.setVisible(true);
    }
}
