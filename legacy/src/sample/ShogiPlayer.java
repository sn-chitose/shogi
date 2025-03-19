package sample;

import sample.tiles.*;

import javax.swing.*;
import java.awt.*;
import java.awt.event.*;
import java.awt.geom.AffineTransform;
import java.util.ArrayList;
import java.util.stream.Collectors;

class ShogiPlayer extends JLabel implements Runnable {

    private static final long FPS = 60;

    private ArrayList<Tile> player1caught, player2caught;
    private ArrayList<int[]> movable, dangerous;
    private Tile[][] field;

    private Tile selected;
    private boolean turnPlayer2, moverInCheck;
    private Point cursor;
    private Color font, tileC;

    private boolean selectDrops = false;
    private boolean inputable = false;
    private boolean onSelect = false;
    private long timeOnKey = Long.MAX_VALUE;
    private short displayTime = Short.MAX_VALUE;
    private KeyEvent lastKey = null;

    ShogiPlayer(Color front, Color back, Color tileback) {
        setSize(640, 480);
        setBackground(back);
        setDoubleBuffered(true);
        setFocusable(true);
        font = front;
        tileC = tileback;
        player1caught = new ArrayList<>();
        player2caught = new ArrayList<>();
        movable = new ArrayList<>();
        dangerous = new ArrayList<>();
        field = new Tile[9][9];
        selected = null;
        turnPlayer2 = false;
        cursor = new Point(4, 7);
        initTiles();
        initListeners();

    }

    private void initTiles() {
        for (Tile tile : new Tile[] {
                new Gyokushou(4, 8, false),
                new Hisha(7, 7, false),
                new Kakugyou(1, 7, false),
                new Kinshou(3, 8, false),
                new Kinshou(5, 8, false),
                new Ginshou(2, 8, false),
                new Ginshou(6, 8, false),
                new Keima(1, 8, false),
                new Keima(7, 8, false),
                new Kyousha(0, 8, false),
                new Kyousha(8, 8, false),

                new Gyokushou(4, 0, true),
                new Hisha(1, 1, true),
                new Kakugyou(7, 1, true),
                new Kinshou(3, 0, true),
                new Kinshou(5, 0, true),
                new Ginshou(2, 0, true),
                new Ginshou(6, 0, true),
                new Keima(1, 0, true),
                new Keima(7, 0, true),
                new Kyousha(0, 0, true),
                new Kyousha(8, 0, true)})
            field[tile.getX()][tile.getY()] = tile;
        for (int x = 0; x < 9; x++) {
            field[x][6] = new Fuhyou(x, 6, false);
            field[x][2] = new Fuhyou(x, 2, true);
        }
    }

    private void initListeners() {
        addKeyListener(new KeyListener() {

            @Override
            public void keyTyped(KeyEvent e) {}

            @Override
            public void keyPressed(KeyEvent e) {
                if (inputable) {
                    if (lastKey != null) if (!lastKey.equals(e)) {
                        lastKey = e;
                        timeOnKey = System.currentTimeMillis();
                    } else if (System.currentTimeMillis() - timeOnKey < 500) return;
                    switch (e.getKeyCode()) {
                        case KeyEvent.VK_UP:
                            if (selectDrops && !onSelect)
                                selectDropListUp();
                            else {
                                cursor.y--;
                                if (cursor.y < 0) cursor.y = 8;
                            }
                            break;
                        case KeyEvent.VK_DOWN:
                            if (selectDrops && !onSelect)
                                selectDropListDown();
                            else {
                                cursor.y++;
                                if (cursor.y > 8) cursor.y = 0;
                            }
                            break;
                        case KeyEvent.VK_RIGHT:
                            if (selectDrops && !onSelect)
                                selectDropListDown();
                            else {
                                cursor.x++;
                                if (cursor.x > 8) cursor.x = 0;
                            }
                            break;
                        case KeyEvent.VK_LEFT:
                            if (selectDrops && !onSelect)
                                selectDropListUp();
                            else {
                                cursor.x--;
                                if (cursor.x < 0) cursor.x = 8;
                            }
                            break;
                        case KeyEvent.VK_SPACE:
                            selected = null;
                            movable.clear();
                            onSelect = false;
                            if (!(turnPlayer2 ? player2caught : player1caught).isEmpty())
                                if (selectDrops) {
                                    cursor.x = 4;
                                    cursor.y = 4;
                                    selectDrops = false;
                                } else {
                                    cursor.x = tileToInt((turnPlayer2 ? player2caught : player1caught).get(0).getClass());
                                    selectDrops = true;
                                }
                            break;
                        case KeyEvent.VK_ENTER:
                            select();
                            break;
                        default:
                            return;
                    }
                    repaint();
                }
            }

            @Override
            public void keyReleased(KeyEvent e) {
                lastKey = null;
                timeOnKey = Long.MAX_VALUE;
            }
        });
    }

    @Override
    public void addNotify() {
        super.addNotify();
        Thread thread = new Thread(this);
        thread.start();
    }

    private void select() {
        if (selected != null && cursor.x == selected.getX() && cursor.y == selected.getY()) return;
        if (onSelect) {
            if (selectDrops) dropTile(cursor.x, cursor.y);
            else selectMove(cursor.x, cursor.y);
        }
        else {
            if (selectDrops) selectDrop(cursor.x);
            else selectTile(cursor.x, cursor.y);
        }
    }

    private void selectDropListUp() {
        while (true) {
            cursor.x = (cursor.x + 7) % 8;
            for (Tile tile : (turnPlayer2 ? player2caught : player1caught))
                if (tileToInt(tile.getClass()) == cursor.x)
                    return;
        }
    }

    private void selectDropListDown() {
        while (true) {
            cursor.x = (cursor.x + 1) % 8;
            for (Tile tile : (turnPlayer2 ? player2caught : player1caught))
                if (tileToInt(tile.getClass()) == cursor.x)
                    return;
        }
    }

    private boolean selectTile(int x, int y) {
        selected = field[x][y];
        if (selected == null) return false;
        if (selected.isPlayer2() != turnPlayer2) {
            selected = null;
            return false;
        }
        calcMovable();
        excludeDangerousMoves();
        onSelect = true;
        return true;
    }

    private boolean selectMove(int x, int y) {
        for (int[] point : movable)
            if (point[0] == x && point[1] == y) {
                moveTile(x, y);
                onSelect = false;
                isInCheck(true);
                turnPlayer2 = !turnPlayer2;
                selected = null;
                movable.clear();
                return true;
            }
        movable.clear();
        return selectTile(x, y);
    }

    private void calcMovable() {
        movable.clear();
        ArrayList<Integer> run = new ArrayList<>();
        if (selectDrops) {
            for (int x = 0; x < 9; x++)
                for (int y = 0; y < 9; y++)
                    if (droppable(x, y))
                        movable.add(new int[]{x, y});
        }
        else {
            for (int dir : selected.isPromoted() ? selected.getPromotedMoves() : selected.getMoves()) {
                int dx = 0, dy = 0;
                switch (dir) {
                    case 1:
                        dx = -1;
                        dy = 1;
                        break;
                    case 2:
                        dy = 1;
                        break;
                    case 3:
                        dx = 1;
                        dy = 1;
                        break;
                    case 4:
                        dx = -1;
                        break;
                    case 6:
                        dx = 1;
                        break;
                    case 7:
                        dx = -1;
                        dy = -1;
                        break;
                    case 8:
                        dy = -1;
                        break;
                    case 9:
                        dx = 1;
                        dy = -1;
                        break;
                    case 27:
                        dx = -1;
                        dy = -2;
                        break;
                    case 29:
                        dx = 1;
                        dy = -2;
                        break;
                    default:
                        run.add(dir);
                }
                int nx = selected.getX(), ny = selected.getY();
                if (turnPlayer2) {
                    nx -= dx;
                    ny -= dy;
                } else {
                    nx += dx;
                    ny += dy;
                }
                canGo(nx, ny);
            }
            for (int dir : run) {
                int dx = 0, dy = 0;
                switch (dir) {
                    case 11:
                        dx = -1;
                        dy = 1;
                        break;
                    case 22:
                        dy = 1;
                        break;
                    case 33:
                        dx = 1;
                        dy = 1;
                        break;
                    case 44:
                        dx = -1;
                        break;
                    case 66:
                        dx = 1;
                        break;
                    case 77:
                        dx = -1;
                        dy = -1;
                        break;
                    case 88:
                        dy = -1;
                        break;
                    case 99:
                        dx = 1;
                        dy = -1;
                        break;
                }
                int nx = selected.getX(), ny = selected.getY();
                do {
                    if (turnPlayer2) {
                        nx -= dx;
                        ny -= dy;
                    } else {
                        nx += dx;
                        ny += dy;
                    }
                } while (canGo(nx, ny));
            }
        }
    }

    private boolean canGo(int nx, int ny) {
        if (nx < 0 || ny < 0 || nx > 8 || ny > 8) return false;
        Tile target = field[nx][ny];
        if (target != null && target.isPlayer2() == turnPlayer2) return false;
        movable.add(new int[]{nx, ny});
        return target == null;
    }

    private void excludeDangerousMoves() {
        ArrayList<int[]> oldMoves = new ArrayList<>();
        ArrayList<int[]> forbidden = new ArrayList<>();
        oldMoves.addAll(movable.stream().map(int[]::clone).collect(Collectors.toList()));
        forbidden.addAll(oldMoves.stream().filter(pos -> isDangerousMove(pos[0], pos[1])).collect(Collectors.toList()));
        oldMoves.removeAll(forbidden);
        movable = oldMoves;
    }

    private boolean isDangerousMove(int nx, int ny) {
        Tile target = field[nx][ny];
        int ox = selected.getX();
        int oy = selected.getY();
        field[nx][ny] = selected;
        if (ox >= 0 || oy >= 0)
            field[ox][oy] = null;
        dangerous.clear();
        turnPlayer2 = !turnPlayer2;
        for (Tile[] row : field) for (Tile tile : row) if (tile != null && tile.isPlayer2() == turnPlayer2) {
            selected = tile;
            calcMovable();
            dangerous.addAll(movable);
        }
        turnPlayer2 = !turnPlayer2;

        if (field[nx][ny] instanceof Gyokushou) {
            for (int[] pos : dangerous) if (pos[0] == nx && pos[1] == ny) {
                field[ox][oy] = field[nx][ny];
                field[nx][ny] = target;
                selected = field[ox][oy];
                return true;
            }
            field[ox][oy] = field[nx][ny];
            field[nx][ny] = target;
            selected = field[ox][oy];
            return false;
        }
        for (Tile[] row : field) for (Tile tile : row)
            if (tile != null && tile instanceof Gyokushou && tile.isPlayer2() == turnPlayer2) {
                for (int[] pos : dangerous) if (tile.getX() == pos[0] && tile.getY() == pos[1]) {
                    if (ox >= 0 || oy >= 0)
                        field[ox][oy] = field[nx][ny];
                    else selected = field[nx][ny];
                    field[nx][ny] = target;
                    if (ox >= 0 || oy >= 0)
                        selected = field[ox][oy];
                    return true;
                }
                if (ox >= 0 || oy >= 0)
                    field[ox][oy] = field[nx][ny];
                else selected = field[nx][ny];
                field[nx][ny] = target;
                if (ox >= 0 || oy >= 0)
                    selected = field[ox][oy];
                return false;
            }
        return false;
    }

    private void moveTile(int x, int y) {
        movable.clear();
        Tile tile = field[x][y];
        if (tile != null && tile.isPlayer2() != turnPlayer2) {
            tile.initialize();
            (turnPlayer2 ? player2caught : player1caught).add(tile);
            tile.setPlayer(!tile.isPlayer2());
        }
        int ox = selected.getX();
        int oy = selected.getY();
        selected.setX(x);
        selected.setY(y);
        field[ox][oy] = null;
        field[x][y] = selected;

        if (selected.getPromotedMoves() != null && !selected.isPromoted() &&
                (!turnPlayer2 && (oy < 3 || y < 3) || turnPlayer2 && (oy > 5 || y > 5))) {
            if ((!turnPlayer2 && (oy <= 1 || y <= 1) || (turnPlayer2 && (oy >= 7 || y >= 7))) &&
                    selected instanceof Keima ||
                    ((!turnPlayer2 && (oy == 0 || y == 0) || (turnPlayer2 && (oy == 8 || y == 8))) &&
                            (selected instanceof Fuhyou || selected instanceof Kyousha)))
                promote();
            else if (JOptionPane.showConfirmDialog(this, "Promote this tile?\nこの駒を成りますか？",
                    "", JOptionPane.YES_NO_OPTION) == JOptionPane.OK_OPTION)
                promote();
        }
        selected = null;
    }

    private boolean selectDrop(int x) {
        for (Tile tile : (turnPlayer2 ? player2caught : player1caught))
            if (tileToInt(tile.getClass()) == x) {
                selected = tile;
                calcMovable();
                excludeDangerousMoves();
                onSelect = true;
                cursor.x = 4;
                cursor.y = 4;
                return true;
            }
        return false;
    }

    private boolean dropTile(int x, int y) {
        if (droppable(x, y)) {
            (turnPlayer2 ? player2caught : player1caught).remove(selected);
            field[x][y] = selected;
            selected.setX(x);
            selected.setY(y);
            onSelect = false;
            selectDrops = false;
            isInCheck(true);
            turnPlayer2 = !turnPlayer2;
            selected = null;
            movable.clear();
            cursor.x = 4;
            cursor.y = 4;
            return true;
        }
        return false;
    }

    private boolean droppable(int x, int y) {
        if (field[x][y] != null) return false;
        if (selected instanceof Fuhyou)
            for (Tile tile : field[x])
                if (tile != null && tile.isPlayer2() == turnPlayer2 && tile instanceof Fuhyou)
                    return false;
        if (selected instanceof Fuhyou || selected instanceof Kyousha)
            if (!turnPlayer2 && y == 0 || turnPlayer2 && y == 8) return false;
        if (selected instanceof Keima)
            if (!turnPlayer2 && y < 2 || turnPlayer2 && y > 6) return false;
        return true;
    }

    private void promote() {
        selected.promote();
    }

    private void isInCheck(boolean showMessage) {
        dangerous.clear();
        for (Tile[] row : field) for (Tile tile : row) if (tile != null && tile.isPlayer2() == turnPlayer2) {
            selected = tile;
            calcMovable();
            dangerous.addAll(movable);
        }
        selected = null;
        for (Tile[] row : field) for (Tile tile : row)
            if (tile != null && tile instanceof Gyokushou && tile.isPlayer2() != turnPlayer2)
                for (int[] pos : dangerous) if (tile.getX() == pos[0] && tile.getY() == pos[1]) {
                    if (isCheckmate()) return;
                    check(showMessage);
                    return;
                }
        if (showMessage) moverInCheck = false;
    }

    private boolean isCheckmate() {
        turnPlayer2 = !turnPlayer2;
        for (Tile[] row : field) for (Tile tile : row) if (tile != null && tile.isPlayer2() == turnPlayer2) {
            selected = tile;
            calcMovable();
            excludeDangerousMoves();
            if (!movable.isEmpty()) {
                turnPlayer2 = !turnPlayer2;
                selected = null;
                movable.clear();
                dangerous.clear();
                return false;
            }
        }
        for (Tile drop : turnPlayer2 ? player2caught : player1caught) {
            selected = drop;
            for (int x = 0; x < 9; x++) for (int y = 0; y < 9; y++)
                if (droppable(x, y) && !isDangerousMove(x, y))
                    movable.add(new int[]{x, y});
        }
        turnPlayer2 = !turnPlayer2;
        selected = null;
        if (!movable.isEmpty()) {
            movable.clear();
            dangerous.clear();
            return false;
        }
        dangerous.clear();
        checkmate();
        return true;
    }

    private void checkmate() {
        inputable = false;
        JOptionPane.showMessageDialog(this, "Checkmate. " + (turnPlayer2 ? "Player 2" : "Player 1") + " wins.\n" +
                "詰めろ。" + (turnPlayer2 ? "後手" : "先手") + "の勝利", "", JOptionPane.PLAIN_MESSAGE);
    }


    private void check(boolean showMessage) {
        if (showMessage) displayTime = 0;
        moverInCheck = true;
    }

    private int tileToInt(Class type) {
        if (type.equals(Fuhyou.class)) return 0;
        if (type.equals(Kyousha.class)) return 1;
        if (type.equals(Keima.class)) return 2;
        if (type.equals(Ginshou.class)) return 3;
        if (type.equals(Kinshou.class)) return 4;
        if (type.equals(Kakugyou.class)) return 5;
        if (type.equals(Hisha.class)) return 6;
        return -1;
    }

    private int[] getCaughtPos(int t, boolean forPlayer2) {
        int[] pos = new int[2];
        if (!forPlayer2) {
            if (t == 0)
                pos[0] = 675;
            else if (t % 2 == 1)
                pos[0] = 650;
            else pos[0] = 700;
            switch (t) {
                case 0:
                    pos[1] = 304;
                    break;
                case 1:case 2:
                    pos[1] = 354;
                    break;
                case 3:case 4:
                    pos[1] = 404;
                    break;
                case 5:case 6:
                    pos[1] = 454;
                    break;
            }
        }
        else {
            if (t == 0)
                pos[0] = 75;
            else if (t % 2 == 1)
                pos[0] = 100;
            else pos[0] = 50;
            switch (t) {
                case 0:
                    pos[1] = 204;
                    break;
                case 1:case 2:
                    pos[1] = 154;
                    break;
                case 3:case 4:
                    pos[1] = 104;
                    break;
                case 5:case 6:
                    pos[1] = 54;
                    break;
            }
        }
        return pos;
    }

    @Override
    protected void paintComponent(Graphics g) {
        g.fillRect(0,0,getWidth(),getHeight());
        g.setColor(font);
        int x = 175, y = 55;
        while (x < 650) {
            g.drawLine(175, y, 625, y);
            g.drawLine(x, 55, x, 505);
            x += 50;
            y += 50;
        }
        if (selectDrops && !onSelect)
            g.drawImage(getToolkit().getImage("cursor1.png"), getCaughtPos(cursor.x, turnPlayer2)[0],
                    getCaughtPos(cursor.x, turnPlayer2)[1], null);
        else
            g.drawImage(getToolkit().getImage("cursor1.png"), 175 + cursor.x * 50, 55 + cursor.y * 50, null);
        Image mov = getToolkit().getImage("cursor2.png");
        Image target = getToolkit().getImage("cursor3.png");

        if (selected != null) {
            if (selectDrops)
                g.drawImage(getToolkit().getImage("cursor4.png"), getCaughtPos(tileToInt(selected.getClass()), turnPlayer2)[0],
                        getCaughtPos(tileToInt(selected.getClass()), turnPlayer2)[1], null);
            else
                g.drawImage(getToolkit().getImage("cursor4.png"), 175 + selected.getX() * 50, 54 + selected.getY() * 50, null);
        }
        for (int[] pos : movable)
            g.drawImage((field[pos[0]][pos[1]] == null ? mov : target), 175 + pos[0] * 50, 54 + pos[1] * 50, null);

        Image tileback = getToolkit().getImage(tileC.equals(Color.black) ? "tiles\\tile_black.png" : "tiles\\tile_white.png");
        for (Tile[] row : field) for (Tile tile : row) if (tile != null) {
            Image tiletext = getToolkit().getImage(getSpriteName(tile));
            if (tile.isPlayer2()) {
                AffineTransform at = new AffineTransform();
                at.translate(200 + tile.getX() * 50, 81 + tile.getY() * 50);
                at.rotate(Math.PI);
                at.translate(-25, -25);
                ((Graphics2D)g).drawImage(tileback, at, null);
                ((Graphics2D)g).drawImage(tiletext, at, null);
            } else {
                g.drawImage(tileback, 175 + tile.getX() * 50, 54 + tile.getY() * 50, null);
                g.drawImage(tiletext, 175 + tile.getX() * 50, 54 + tile.getY() * 50, null);
            }
        }
        for (Tile tile : player1caught) {
            Image tiletext = getToolkit().getImage(getSpriteName(tile));
            g.drawImage(tileback, getCaughtPos(tileToInt(tile.getClass()), false)[0],
                    getCaughtPos(tileToInt(tile.getClass()), false)[1], null);
            g.drawImage(tiletext, getCaughtPos(tileToInt(tile.getClass()), false)[0],
                    getCaughtPos(tileToInt(tile.getClass()), false)[1], null);
        }
        for (Tile tile : player2caught) {
            Image tiletext = getToolkit().getImage(getSpriteName(tile));
            AffineTransform at = new AffineTransform();
            at.translate(getCaughtPos(tileToInt(tile.getClass()), true)[0] + 25,
                    getCaughtPos(tileToInt(tile.getClass()), true)[1] + 27);
            at.rotate(Math.PI);
            at.translate(-25, -25);
            ((Graphics2D)g).drawImage(tileback, at, null);
            ((Graphics2D)g).drawImage(tiletext, at, null);
        }

        if (displayTime < 90) {
            g.setColor(Color.red);
            int size = displayTime < 75 ? 48 : 48 - 3 * (displayTime - 75);
            g.setFont(new Font("EPSON 行書体Ｍ", Font.PLAIN, size));
            g.drawString("王手！！", 350, 320);
            displayTime++;
        } else if (moverInCheck) {
            g.setColor(Color.red);
            g.setFont(new Font("EPSON 行書体Ｍ", Font.PLAIN, 32));
            g.drawString("王手！！", 350, 550);
        }
    }

    private String getSpriteName(Tile tile) {
        String text = "tiles\\";
        if (tile.isPromoted()) {
            if (tile instanceof Hisha) text += "ryuuou";
            else if (tile instanceof Kakugyou) text += "ryuuma";
            else if (tile instanceof Ginshou) text += "narigin";
            else if (tile instanceof Keima) text += "narikei";
            else if (tile instanceof Kyousha) text += "narikyou";
            else if (tile instanceof Fuhyou) text += "tokin";
        } else {
            if (tile instanceof Gyokushou) text += tile.isPlayer2() ? "oushou" : "gyokushou";
            else if (tile instanceof Hisha) text += "hisha";
            else if (tile instanceof Kakugyou) text += "kakugyou";
            else if (tile instanceof Kinshou) text += "kinshou";
            else if (tile instanceof Ginshou) text += "ginshou";
            else if (tile instanceof Keima) text += "keima";
            else if (tile instanceof Kyousha) text += "kyousha";
            else if (tile instanceof Fuhyou) text += "fuhyou";

        }
        text += font.equals(Color.white) ? "_white.png" : "_black.png";
        return text;
    }

    @Override
    public void run() {
        long TPS = 1000 / FPS;
        long startTime, sleepTime;
        inputable = true;
        while (true) {
            startTime = System.currentTimeMillis();
            repaint();
            sleepTime = TPS - (System.currentTimeMillis() - startTime);
            try {
                Thread.sleep(sleepTime > 0 ? sleepTime : 10);
            } catch (Exception e) {
                System.out.println("Interrupted: " + e.getMessage());
            }
        }
    }
}