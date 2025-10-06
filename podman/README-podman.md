### ⚙️ Podman with Make

```bash
# build + run container
make podman-run

# OR run as a Pod (kube manifest style)
make podman-up

# stop / remove
make podman-down

# view logs
make podman-logs

# shell inside container
make podman-shell

# generate & enable systemd service
make podman-systemd
