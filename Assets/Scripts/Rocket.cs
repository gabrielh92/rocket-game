using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Rocket : MonoBehaviour
{
    [SerializeField] float rcsThrust = 100f;
    [SerializeField] float mainThrust = 25f;

    [SerializeField] AudioClip thrustSfx;
    [SerializeField] AudioClip deathSfx;
    [SerializeField] AudioClip finishSfx;

    [SerializeField] ParticleSystem thrustParticles;
    [SerializeField] ParticleSystem deathParticles;
    [SerializeField] ParticleSystem finishParticles;
    [SerializeField] float levelLoadDelay = 2.5f;

    [SerializeField] TextMeshProUGUI uiLevelText;
    [SerializeField] TextMeshProUGUI uiDeathsText;

    enum State { ALIVE, DEAD, TRANSITION };

    Rigidbody rb;
    AudioSource audioSource;
    State state = State.ALIVE;
    bool canCollide = true;

    private void OnEnable() {
        SceneManager.sceneLoaded += OnSceneEnabled;
    }

    private void OnSceneEnabled(Scene _scene, LoadSceneMode _mode) {
        uiLevelText.text = $"{_scene.buildIndex}";
        int _deaths = -1;
        if(GameManager.instance != null) {
            _deaths = GameManager.instance.GetDeaths(_scene.buildIndex);
        }
        uiDeathsText.text = $"Deaths {_deaths}";
    }

    void Start() {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
    }

    void Update() {
        if(state == State.ALIVE){
            Thrust();
            Move();
        }

        if(Debug.isDebugBuild) {
            RespondToDebugKeys();
        }
    }

    void OnCollisionEnter(Collision collision) {
        if(state != State.ALIVE || !canCollide) return;
        switch (collision.gameObject.tag) {
            case "Friendly":
                break;
            case "Finish":
                state = State.TRANSITION;
                audioSource.Stop();
                audioSource.PlayOneShot(finishSfx);
                finishParticles.Play();
                Invoke("OnFinish", levelLoadDelay);
                break;
            default:
                state = State.DEAD;
                audioSource.Stop();
                audioSource.PlayOneShot(deathSfx);
                deathParticles.Play();
                Invoke("OnDeath", levelLoadDelay);
                break;
        }
    }

    private void OnFinish() {
        if(GameManager.instance != null) {
            GameManager.instance.LoadNextLevel();
        }
    }

    private void OnDeath() {
        if(GameManager.instance != null) {
            GameManager.instance.AddDeath();
            GameManager.instance.LoadCurrentLevel();
        } else {
            Debug.LogWarning("Game Manager instance not defined");
        }
    }

    private void Move() {
        rb.angularVelocity = Vector3.zero; // take manual control of rotation
        float rotationSpeed = rcsThrust * Time.deltaTime;
        if(Input.GetKey(KeyCode.A)) {
            transform.Rotate(rotationSpeed * Vector3.forward);
        } else if(Input.GetKey(KeyCode.D)) {
            transform.Rotate(rotationSpeed * -Vector3.forward);
        }
    }

    private void Thrust() {
        if(Input.GetKey(KeyCode.Space)) {
            rb.AddRelativeForce(mainThrust * Time.deltaTime * Vector3.up);
            if(!audioSource.isPlaying) {
                audioSource.PlayOneShot(thrustSfx);
            }
            thrustParticles.Play();
        } else {
            audioSource.Stop();
            thrustParticles.Stop();
        }
    }

    private void RespondToDebugKeys() {
#if UNITY_EDITOR
        if(Input.GetKeyDown(KeyCode.L)) {
            GameManager.instance.LoadNextLevel();
        }
        if(Input.GetKeyDown(KeyCode.C)) {
            SetCollision(!canCollide);
        }
#endif
    }

    public void SetCollision(bool _state) {
        canCollide = _state;
    }

    public bool IsDead() {
        return state == State.DEAD;
    }
}
