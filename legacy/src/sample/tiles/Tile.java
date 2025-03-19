package sample.tiles;

/**
 * Created by Alex on 23.09.2015.
 */
public abstract class Tile {

    protected int x, y, moves[] = null, promoted[] = null;
    protected boolean isPromoted, player2;

    public Tile(int x, int y, boolean player2) {
        this.x = x;
        this.y = y;
        isPromoted = false;
        this.player2 = player2;
        initMoves();
    }

    protected abstract void initMoves();

    public int getX() {
        return x;
    }

    public void setX(int x) {
        this.x = x;
    }

    public int getY() {
        return y;
    }

    public void setY(int y) {
        this.y = y;
    }

    public boolean isPlayer2() {
        return player2;
    }

    public void setPlayer(boolean isPlayer2) {
        player2 = isPlayer2;
    }

    public boolean isPromoted() {
        return isPromoted;
    }

    public void promote() {
        isPromoted = true;
    }

    public int[] getMoves() {
        return moves;
    }

    public int[] getPromotedMoves() {
        return promoted;
    }

    public void initialize() {
        x = -1;
        y = -1;
        isPromoted = false;
    }

    public abstract String getTile();
}
