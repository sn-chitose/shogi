package sample.tiles;

/**
 * Created by Alex on 23.09.2015.
 */
public class Kakugyou extends Tile {

    public Kakugyou(int x, int y, boolean player2) {
        super(x, y, player2);
    }

    @Override
    protected void initMoves() {
        moves = new int[]{11,33,77,99};
        promoted = new int[]{2,4,6,8,11,33,77,99};
    }

    @Override
    public String getTile() {
        return "kaku";
    }
}
