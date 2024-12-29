using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public partial class Mole : MonoBehaviour {
  
  [Header("Graphics")]
  [SerializeField] private Sprite mole;
  [SerializeField] private Sprite moleHardHat;
  [SerializeField] private Sprite moleHatBroken;
  [SerializeField] private Sprite moleHit;
  [SerializeField] private Sprite moleHatHit;

    [Header("GameManager1")]


  // The offset of the sprite to hide it.
  private Vector2 startPosition = new Vector2(0f, -2.56f);
  private Vector2 endPosition = Vector2.zero;
  // How long it takes to show a mole.
  private float showDuration = 0.5f;
  private float duration = 1f;

  private SpriteRenderer spriteRenderer;
  private Animator animator;
  private BoxCollider2D boxCollider2D;
  private Vector2 boxOffset;
  private Vector2 boxSize;
  private Vector2 boxOffsetHidden;
  private Vector2 boxSizeHidden;
 


  // Mole Parameters.
  private bool hittable = true;
  private enum MoleType { Standard, HardHat, Bomb };
  private MoleType moleType;
  private float hardRate = 0.25f;
  private float bombRate = 0f;
  private int lives;
  private int moleIndex = 0;

  // Game Manager Reference (assign in Inspector)
  [SerializeField] private GameManager1 gameManager;

  private IEnumerator ShowHide(Vector2 start, Vector2 end) {
    // Make sure we start at the start.
    transform.localPosition = start;

    // Show the mole.
    float elapsed = 0f;
    while (elapsed < showDuration) {
      transform.localPosition = Vector2.Lerp(start, end, elapsed / showDuration);
      boxCollider2D.offset = Vector2.Lerp(boxOffsetHidden, boxOffset, elapsed / showDuration);
      boxCollider2D.size = Vector2.Lerp(boxSizeHidden, boxSize, elapsed / showDuration);
      // Update at max framerate.
      elapsed += Time.deltaTime;
      yield return null;
    }

    // Make sure we're exactly at the end.
    transform.localPosition = end;
    boxCollider2D.offset = boxOffset;
    boxCollider2D.size = boxSize;

    // Wait for duration to pass.
    yield return new WaitForSeconds(duration);

    // Hide the mole.
    elapsed = 0f;
    while (elapsed < showDuration) {
      transform.localPosition = Vector2.Lerp(end, start, elapsed / showDuration);
      boxCollider2D.offset = Vector2.Lerp(boxOffset, boxOffsetHidden, elapsed / showDuration);
      boxCollider2D.size = Vector2.Lerp(boxSize, boxSizeHidden, elapsed / showDuration);

      // Update at max framerate.
      elapsed += Time.deltaTime;
      yield return null;
    }
    // Make sure we are exactly back at the start position.
    transform.localPosition = start;
    boxCollider2D.offset = boxOffsetHidden;
    boxCollider2D.size = boxSizeHidden;

    if (hittable) {
        hittable = false;

        gameManager.Missed(moleIndex, moleType != MoleType.Bomb);
    }

  }

  private IEnumerator QuickHide() {
    yield return new WaitForSeconds(0.25f);
    // Whilst we were waiting we may have spawned again here, so just
    // check that hasn't happened before hiding it. This will stop it
    // flickering in that case.
    if (!hittable) {
      Hide();
    }
  }

  private void OnMouseDown() {
    if (hittable) {
      switch (moleType) {
        case MoleType.Standard:
          spriteRenderer.sprite = moleHit;
          gameManager.AddScore(moleIndex);
          // Stop the animation
          StopAllCoroutines();
          StartCoroutine(QuickHide());
          // Turn off hittable so that we can't keep tapping for score.
          hittable = false;
          break;
        
        case MoleType.HardHat:
        // If lives == 2 reduce, nd change sprite.
        if (lives == 2) {
            spriteRenderer.sprite = moleHatBroken;
            lives--;
        } else {

          spriteRenderer.sprite = moleHatHit;
          gameManager.AddScore(moleIndex);
            // Stop the animation
          StopAllCoroutines();
          StartCoroutine(QuickHide());
          // Turn off hittable so that we can't try keep tapping for more.
          hittable = false;
        }
          break;
          case MoleType.Bomb:
          // Game over, 1 for bomb.
          gameManager.GameOver(1);
          break;
          default:
          break;
      }
    }
  }

  private void CreateNext() {
    float random = UnityEngine.Random.Range(0f, 1f);
    if (random < bombRate) {
        moleType = MoleType.Bomb;
        animator.enabled = true;
    } else {
        animator.enabled = false;
    random = UnityEngine.Random.Range(0f, 1f);
     if (random < bombRate + hardRate) {
        //Create a hard one
        moleType = MoleType.HardHat;
        spriteRenderer.sprite = moleHardHat;
        lives = 2;
    } else {
        //Create a standard one.
        moleType = MoleType.Standard;
        spriteRenderer.sprite = mole;
        lives = 1;
    }
    // mark as hittable so we can register an onclick event.
    hittable = true;
  }  
  }
    private void SetLevel(int Level) {
        bombRate = Mathf.Min(Level * 0.005f, 0.5f);

        hardRate = Mathf.Min(Level * 0.025f, 1f);

        float durationMin = Mathf.Clamp(1 - Level * 0.1f, 0.01f, 1f);
        float durationMax = Mathf.Clamp(2 - Level, 0.1f, 1f); 
        duration = UnityEngine.Random.Range(durationMin, durationMax); 
        }

  public void Hide() {
    transform.localPosition = startPosition;
    hittable = true;
    boxCollider2D.offset = boxOffsetHidden;
    boxCollider2D.size = boxSizeHidden;
  }

  private void Awake() {
    // Get references to the components we'll need.
    spriteRenderer = GetComponent<SpriteRenderer>();
    animator = GetComponent<Animator>();
    boxCollider2D = GetComponent<BoxCollider2D>();

    boxOffset = boxCollider2D.offset;
    boxSize = boxCollider2D.size;
    boxOffsetHidden = new Vector2(boxOffset.x, -startPosition.y / 2f);
    boxSizeHidden = new Vector2(boxSize.x, 0f);
  }

public void Activate(int level) {
    SetLevel(level);
    CreateNext();
    StartCoroutine(ShowHide(startPosition, endPosition));
    }

// Used by the game manager to uniquely identify moles. 
  public void SetIndex(int index) {
    moleIndex = index;
  }

    // Used to freeze the game on finish.
  public void StopGame() {
    hittable = false;
    StopAllCoroutines();
  }

}